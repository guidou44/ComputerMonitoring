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
        private const string _xmlConfigPath = @".\Configuration\MonitoringConfiguration.cfg";


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
                output.Enqueue(GetCalculatedValue(target));
            }

            HashSet<MonitoringTarget> notUsedTargets = _target2connector.Select(T2C => T2C.Key.TargetType).Except(targets).ToHashSet();
            foreach (var nonTarget in notUsedTargets) //CleanUp
            {
                var correspondingKey = _target2connector.Where(T2C => T2C.Key.TargetType == nonTarget).SingleOrDefault().Key;
                _target2connector[correspondingKey] = null;
            }
            return output;
        }

        public IEnumerable<MonitoringTarget> GetAllTargets()
        {
            return _target2connector.Where(TAR => (!TAR.Key.ExcludeFromMonitoring ?? true)).Select(TAR => TAR.Key.TargetType);
        }

        public IEnumerable<MonitoringTarget> GetLocalTargets()
        {
            return _target2connector.Where(TAR => !TAR.Key.IsRemote && (!TAR.Key.ExcludeFromMonitoring ?? true)).Select(TAR => TAR.Key.TargetType);
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

        private void SetTargetDict()
        {
            _target2connector = new Dictionary<ComputerRessource, ConnectorBase>();
            var ressourceCollection = XmlHelper.DeserializeConfiguration<RessourceCollection>(_xmlConfigPath);
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
