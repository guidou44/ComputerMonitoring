using Common.Helpers;
using Common.Reports;
using HardwareAccess.Connectors;
using HardwareAccess.Enums;
using HardwareAccess.Exceptions;
using HardwareAccess.Factories;
using HardwareAccess.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HardwareAccess
{
    public class DataManager
    {
        #region Constructor

        private IEnumerable<MonitoringTarget> _initialMonitoringTargets;
        private IDictionary<ComputerRessource, ConnectorBase> _target2connector;
        private const string _XML_CONFIG_PATH = @".\Configuration\MonitoringConfiguration.cfg";


        public DataManager()
        {
            SetMonitoringTargets();
            SetAvailableTargets_Internal();
        }

        #endregion

        #region Public Methods
        public HardwareInformation GetCalculatedValue(MonitoringTarget target)
        {
            var targetKey = _target2connector.Where(T2C => T2C.Key.TargetType == target).SingleOrDefault().Key;
            if (_target2connector[targetKey] == null) _target2connector[targetKey] = ConnectorFactory.InstantiateConnector(targetKey.ConnectorName);
            return _target2connector[targetKey].GetValue(target);
        }

        public IEnumerable<HardwareInformation> GetCalculatedValues(IEnumerable<MonitoringTarget> targets)
        {
            var _targets = new HashSet<MonitoringTarget>(targets); //let 
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
                }
            }

            //CleanUp
            var notUsedTargets = _target2connector.Where(T2C => T2C.Value != null && !targets.Contains(T2C.Key.TargetType))
                                                  .ToDictionary(T2C => T2C.Key, T2C => T2C.Value)
                                                  .Keys
                                                  .ToList();

            foreach (var nonTarget in notUsedTargets) _target2connector[nonTarget] = null;
            return output;
        }

        public IEnumerable<MonitoringTarget> GetAllTargets()
        {
            return GetAvailableTargets_Internal().Where(TAR => (!TAR.Key.ExcludeFromMonitoring ?? true)).Select(TAR => TAR.Key.TargetType);
        }

        public IEnumerable<MonitoringTarget> GetInitialTargets()
        {
            return _target2connector.Where(T2C => _initialMonitoringTargets
                                    .Contains(T2C.Key.TargetType) && T2C.Key.Com_Error == null)
                                    .Select(T2C => T2C.Key.TargetType);
        }

        public IEnumerable<MonitoringTarget> GetLocalTargets()
        {
            return GetAvailableTargets_Internal().Where(TAR => !TAR.Key.IsRemote && (!TAR.Key.ExcludeFromMonitoring ?? true))
                                                 .Select(TAR => TAR.Key.TargetType);
        }

        public IEnumerable<MonitoringTarget> GetRemoteTargets()
        {
            return GetAvailableTargets_Internal().Where(TAR => TAR.Key.IsRemote && (!TAR.Key.ExcludeFromMonitoring ?? true))
                                                 .Select(TAR => TAR.Key.TargetType);
        }

        public bool IsRemoteMonitoringEnabled()
        {
            foreach (var remoteTarget in _target2connector.Where(TAR => TAR.Key.IsRemote))
            {
                var pingable = TryPing(remoteTarget.Key.RemoteIp);
                if (!pingable) return false;
            }
            return _target2connector.Any(TAR => TAR.Key.IsRemote);
        }

        #endregion

        #region Private Methods

        private IDictionary<ComputerRessource, ConnectorBase> GetAvailableTargets_Internal()
        {
            return _target2connector.Where(T2C => T2C.Key.Com_Error == null).ToDictionary(T2C => T2C.Key, T2C => T2C.Value);
        }

        private void SetAvailableTargets_Internal()
        {
            IEnumerable<ComputerRessource> t2CKeys;
            t2CKeys = (IsRemoteMonitoringEnabled()) ? _target2connector.Keys.ToList() : _target2connector.Where(T2C => !T2C.Key.IsRemote).Select(T2C => T2C.Key).ToList();

            foreach (var target in t2CKeys)
            {
                try
                {
                    var testValue = GetCalculatedValue(target.TargetType);
                    if (testValue == null) throw new HardwareCommunicationException(target.TargetType);
                }
                catch (Exception e) { _target2connector.SingleOrDefault(T2C => T2C.Key == target).Key.Com_Error = e; }
            }
        }

        private void SetMonitoringTargets()
        {
            _target2connector = new Dictionary<ComputerRessource, ConnectorBase>();
            var ressourceCollection = XmlHelper.DeserializeConfiguration<RessourceCollection>(_XML_CONFIG_PATH);
            foreach (var ressource in ressourceCollection.Ressources) _target2connector.Add(ressource, null);
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
