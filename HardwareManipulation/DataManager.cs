using Common.Helpers;
using Common.Reports;
using HardwareManipulation.Connectors;
using HardwareManipulation.Enums;
using HardwareManipulation.Factories;
using HardwareManipulation.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation
{
    public class DataManager
    {
        #region Constructor

        private IDictionary<ComputerRessource, ConnectorBase> _target2connector;
        private const string XML_CONFIG_PATH = @".\Configuration\MonitoringConfiguration.cfg";


        public DataManager()
        {
            SetTargetDict();
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
            var output = new Queue<HardwareInformation>();
            foreach (var target in targets)
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

            HashSet<MonitoringTarget> notUsedTargets = _target2connector.Where(T2C => T2C.Value != null).Select(T2C => T2C.Key.TargetType).Except(targets).ToHashSet();
            foreach (var nonTarget in notUsedTargets) //CleanUp
            {
                var correspondingKey = _target2connector.Where(T2C => T2C.Key.TargetType == nonTarget).SingleOrDefault().Key;
                _target2connector[correspondingKey] = null;
            }
            return output;
        }

        public IEnumerable<MonitoringTarget> GetAllTargets(bool checkAvailability = false)
        {
            var allTargets = _target2connector.Where(TAR => (!TAR.Key.ExcludeFromMonitoring ?? true));
            if(!checkAvailability) return allTargets.Select(TAR => TAR.Key.TargetType);
            return GetAvailableTargets_Internal(allTargets);
        }

        public IEnumerable<MonitoringTarget> GetLocalTargets(bool checkAvailability = false)
        {
            var localTargets = _target2connector.Where(TAR => !TAR.Key.IsRemote && (!TAR.Key.ExcludeFromMonitoring ?? true));
            if (!checkAvailability) return localTargets.Select(TAR => TAR.Key.TargetType);
            return GetAvailableTargets_Internal(localTargets);
        }

        public IEnumerable<MonitoringTarget> GetRemoteTargets()
        {
            return _target2connector.Where(TAR => TAR.Key.IsRemote && (!TAR.Key.ExcludeFromMonitoring ?? true)).Select(TAR => TAR.Key.TargetType);
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

        private IEnumerable<MonitoringTarget> GetAvailableTargets_Internal(IEnumerable<KeyValuePair<ComputerRessource, ConnectorBase>> targets)
        {
            ICollection<MonitoringTarget> availableTargets = new HashSet<MonitoringTarget>();
            foreach (var target in targets)
            {
                try
                {
                    var testValue = GetCalculatedValue(target.Key.TargetType);
                    if (testValue != null) availableTargets.Add(target.Key.TargetType);
                }
                catch (Exception) { }
            }
            return availableTargets;
        }

        private void SetTargetDict()
        {
            _target2connector = new Dictionary<ComputerRessource, ConnectorBase>();
            var ressourceCollection = XmlHelper.DeserializeConfiguration<RessourceCollection>(XML_CONFIG_PATH);
            foreach (var ressource in ressourceCollection.Ressources)
            {
                _target2connector.Add(ressource, null);
            }
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
