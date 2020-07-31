using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using DesktopAssistant.BL.Hardware;
using Hardware.Connectors;
using Hardware.Exceptions;
using Hardware.Models;

namespace Hardware
{
    public class HardwareManager : IHardwareManager
    {
        private const string CONFIG_FILE_PATH = @".\Configuration\MonitoringConfiguration.cfg";
        private IFactory<ConnectorBase> _connectorFactory;

        private IEnumerable<MonitoringTarget> _initialMonitoringTargets;
        private IDictionary<ComputerResource, ConnectorBase> _target2Connector;
        private XmlHelper _xmlHelper;

        public HardwareManager(IFactory<ConnectorBase> factory, XmlHelper xmlHelper, string alternateConfigPath = null)
        {
            this._xmlHelper = xmlHelper;
            this._connectorFactory = factory;
            SetMonitoringTargets(alternateConfigPath ?? CONFIG_FILE_PATH);
            SetAvailableTargets_Internal();
        }

        public virtual IEnumerable<MonitoringTarget> GetInitialTargets()
        {
            return _target2Connector.Where(t2C => _initialMonitoringTargets
                                    .Contains(t2C.Key.TargetType) && t2C.Key.Com_Error == null)
                                    .Select(t2C => t2C.Key.TargetType);
        }

        public virtual IHardwareInfo GetCalculatedValue(MonitoringTarget target)
        {
            var targetKey = _target2Connector.SingleOrDefault(t2C => t2C.Key.TargetType == target).Key;
            if (_target2Connector[targetKey] == null) _target2Connector[targetKey] = _connectorFactory.CreateInstance(targetKey.ConnectorName);
            return _target2Connector[targetKey].GetValue(target);
        }

        public virtual IEnumerable<IHardwareInfo> GetCalculatedValues(ICollection<MonitoringTarget> targets)
        {
            var _targets = new HashSet<MonitoringTarget>(targets);
            var output = new Queue<IHardwareInfo>();
            foreach (var target in _targets)
            {
                try
                {
                    output.Enqueue(GetCalculatedValue(target));
                }
                catch (Exception e)
                {
                    output.Enqueue(new HardwareInformation() { MainValue = 0, UnitSymbol = $"COM ERR: {target.ToString()}", ShortName="ERR"});
                    if (targets.Any())
                        targets.Remove(target);
                }
            }

            var notUsedTargets = _target2Connector.Where(t2C => t2C.Value != null && !targets.Contains(t2C.Key.TargetType))
                                                  .ToDictionary(t2C => t2C.Key, T2C => T2C.Value)
                                                  .Keys
                                                  .ToList();

            foreach (var nonTarget in notUsedTargets) _target2Connector[nonTarget] = null;
            return output;
        }

        public ICollection<MonitoringTarget> GetAllTargets()
        {
            if (IsRemoteMonitoringEnabled())
                return GetAvailableTargets_Internal()
                    .Where(tar => (!tar.Key.ExcludeFromMonitoring ?? true))
                    .Select(TAR => TAR.Key.TargetType).ToList();

            return GetLocalTargets();
        }

        public virtual ICollection<MonitoringTarget> GetLocalTargets()
        {
            return GetAvailableTargets_Internal().Where(tar => !tar.Key.IsRemote && (!tar.Key.ExcludeFromMonitoring ?? true))
                                                 .Select(tar => tar.Key.TargetType).ToList();
        }

        public virtual IEnumerable<MonitoringTarget> GetRemoteTargets()
        {
            return GetAvailableTargets_Internal().Where(tar => tar.Key.IsRemote && (!tar.Key.ExcludeFromMonitoring ?? true))
                                                 .Select(tar => tar.Key.TargetType);
        }

        public virtual bool IsRemoteMonitoringEnabled()
        {
            foreach (KeyValuePair<ComputerResource, ConnectorBase> remoteTarget in _target2Connector.Where(tar => tar.Key.IsRemote))
            {
                bool ping = remoteTarget.Key.TryPing();
                if (!ping) return false;
            }
            return _target2Connector.Any(TAR => TAR.Key.IsRemote);
        }

        #region Private Methods

        private IDictionary<ComputerResource, ConnectorBase> GetAvailableTargets_Internal()
        {
            return _target2Connector.Where(T2C => T2C.Key.Com_Error == null).ToDictionary(T2C => T2C.Key, T2C => T2C.Value);
        }

        private void SetAvailableTargets_Internal()
        {
            IEnumerable<ComputerResource> t2CKeys;
            t2CKeys = (IsRemoteMonitoringEnabled()) ? _target2Connector.Keys.ToList() : _target2Connector.Where(T2C => !T2C.Key.IsRemote).Select(T2C => T2C.Key).ToList();

            foreach (var target in t2CKeys)
            {
                try
                {
                    var testValue = GetCalculatedValue(target.TargetType);
                    if (testValue == null) throw new HardwareCommunicationException(target.TargetType);
                }
                catch (Exception e) { _target2Connector.SingleOrDefault(T2C => T2C.Key == target).Key.Com_Error = e; }
            }
        }

        private void SetMonitoringTargets(string xmlConfigPath)
        {
            _target2Connector = new Dictionary<ComputerResource, ConnectorBase>();
            var ressourceCollection = _xmlHelper.DeserializeConfiguration<ResourceCollection>(xmlConfigPath);
            foreach (var ressource in ressourceCollection.Ressources) _target2Connector.Add(ressource, null);
            _initialMonitoringTargets = ressourceCollection.InitialTargets;
        }

        #endregion
    }
}
