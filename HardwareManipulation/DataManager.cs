using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Common.Helpers;
using Common.Reports;
using HardwareAccess.Connectors;
using HardwareAccess.Enums;
using HardwareAccess.Exceptions;
using HardwareAccess.Factories;
using HardwareAccess.Models;

namespace HardwareManipulation
{
    public class DataManager
    {
        #region Constructor

        private IEnumerable<MonitoringTarget> _initialMonitoringTargets;
        private IDictionary<ComputerRessource, ConnectorBase> _target2Connector;
        private const string XmlConfigPath = @".\Configuration\MonitoringConfiguration.cfg";


        public DataManager()
        {
            SetMonitoringTargets();
            SetAvailableTargets_Internal();
        }

        #endregion

        #region Public Methods
        public HardwareInformation GetCalculatedValue(MonitoringTarget target)
        {
            var targetKey = _target2Connector.SingleOrDefault(t2C => t2C.Key.TargetType == target).Key;
            if (_target2Connector[targetKey] == null) _target2Connector[targetKey] = ConnectorFactory.InstantiateConnector(targetKey.ConnectorName);
            return _target2Connector[targetKey].GetValue(target);
        }

        public IEnumerable<HardwareInformation> GetCalculatedValues(ICollection<MonitoringTarget> targets)
        {
            var _targets = new HashSet<MonitoringTarget>(targets);
            var output = new Queue<HardwareInformation>();
            foreach (var target in _targets)
            {
                try
                {
                    output.Enqueue(GetCalculatedValue(target));
                }
                catch (Exception e)
                {
                    output.Enqueue(new HardwareInformation() { MainValue = 0, UnitSymbol = $"COM ERR: {target.ToString()}", ShortName="ERR"});
                    if (targets.Count() >= 1)
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

        public IEnumerable<MonitoringTarget> GetAllTargets()
        {
            return GetAvailableTargets_Internal().Where(tar => (!tar.Key.ExcludeFromMonitoring ?? true)).Select(TAR => TAR.Key.TargetType);
        }

        public IEnumerable<MonitoringTarget> GetInitialTargets()
        {
            return _target2Connector.Where(t2C => _initialMonitoringTargets
                                    .Contains(t2C.Key.TargetType) && t2C.Key.Com_Error == null)
                                    .Select(t2C => t2C.Key.TargetType);
        }

        public IEnumerable<MonitoringTarget> GetLocalTargets()
        {
            return GetAvailableTargets_Internal().Where(tar => !tar.Key.IsRemote && (!tar.Key.ExcludeFromMonitoring ?? true))
                                                 .Select(tar => tar.Key.TargetType);
        }

        public IEnumerable<MonitoringTarget> GetRemoteTargets()
        {
            return GetAvailableTargets_Internal().Where(tar => tar.Key.IsRemote && (!tar.Key.ExcludeFromMonitoring ?? true))
                                                 .Select(tar => tar.Key.TargetType);
        }

        public bool IsRemoteMonitoringEnabled()
        {
            foreach (var remoteTarget in _target2Connector.Where(tar => tar.Key.IsRemote))
            {
                bool ping = TryPing(remoteTarget.Key.RemoteIp);
                if (!ping) return false;
            }
            return _target2Connector.Any(TAR => TAR.Key.IsRemote);
        }

        #endregion

        #region Private Methods

        private IDictionary<ComputerRessource, ConnectorBase> GetAvailableTargets_Internal()
        {
            return _target2Connector.Where(T2C => T2C.Key.Com_Error == null).ToDictionary(T2C => T2C.Key, T2C => T2C.Value);
        }

        private void SetAvailableTargets_Internal()
        {
            IEnumerable<ComputerRessource> t2CKeys;
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

        private void SetMonitoringTargets()
        {
            _target2Connector = new Dictionary<ComputerRessource, ConnectorBase>();
            var ressourceCollection = XmlHelper.DeserializeConfiguration<RessourceCollection>(XmlConfigPath);
            foreach (var ressource in ressourceCollection.Ressources) _target2Connector.Add(ressource, null);
            _initialMonitoringTargets = ressourceCollection.InitialTargets;
        }

        private bool TryPing(string ipAddress)
        {
            Ping pingHost;
            try
            {
                pingHost = new Ping();
                PingReply reply = pingHost.Send(ipAddress);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception e)
            {
                Reporter.LogException(e);
                return false;
            }
        }

        #endregion


    }
}
