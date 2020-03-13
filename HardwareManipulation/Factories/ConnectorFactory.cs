using HardwareAccess.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareAccess.Factories
{
    public static class ConnectorFactory
    {
        private static string _connectorDirectory = "HardwareAccess.Connectors.";
        private static string _connectorSuffix = "_Connector";

        public static ConnectorBase InstantiateConnector(string connectorName)
        {
            Type connectorType = Type.GetType(_connectorDirectory + connectorName + _connectorSuffix);
            if (connectorType == null) throw new ArgumentException($"Invalid connector name. Found no connector associated with name {connectorName}");
            return (ConnectorBase) Activator.CreateInstance(connectorType);
        }
    }
}
