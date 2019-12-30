using HardwareManipulation.Connectors;
using HardwareManipulation.Enums;
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

        private IDictionary<ComputerRessource, ConnectorBase> _targets;
        private const string _xmlConfigPath = @".\Configuration\MonitoringConfiguration.cfg";


        public DataManager()
        {
            SetTargetDict();
        }

        #endregion

        #region Public Methods

        public IEnumerable<MonitoringTarget> GetAllTargets()
        {
            return _targets.Select(TAR => TAR.Key.TargetName);
        }

        public IEnumerable<MonitoringTarget> GetLocalTargets()
        {
            return _targets.Where(TAR => !TAR.Key.IsRemote).Select(TAR => TAR.Key.TargetName);
        }

        public IEnumerable<MonitoringTarget> GetRemoteTargets()
        {
            return _targets.Where(TAR => TAR.Key.IsRemote).Select(TAR => TAR.Key.TargetName);
        }

        public bool IsRemoteMonitoringEnabled()
        {
            foreach (var remoteTarget in _targets.Where(TAR => TAR.Key.IsRemote))
            {
                var pingable = TryPing(remoteTarget.Key.RemoteIp);
                if (!pingable) return false;
            }
            return _targets.Any(TAR => TAR.Key.IsRemote);
        }

        #endregion

        #region Private Methods

        private void SetTargetDict()
        {
            _targets = new Dictionary<ComputerRessource, ConnectorBase>();
            var ressourceCollection = XmlHelper.DeserializeConfiguration<RessourceCollection>(_xmlConfigPath);
            foreach (var ressource in ressourceCollection.Ressources)
            {
                _targets.Add(ressource, null);
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
                //Reporter.LogException(e);
                return false;
            }
        }

        #endregion


    }
}
