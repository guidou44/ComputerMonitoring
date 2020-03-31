using HardwareAccess.Connectors;
using HardwareManipulation;
using HardwareManipulation.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareAccess.Factories
{
    public class ConnectorFactory : IFactory
    {
        private static string _connectorDirectory = "HardwareAccess.Connectors.";
        private static string _connectorSuffix = "_Connector";

        public ObjectType CreateInstance<ObjectType>(string refName)
        {
            Type connectorType = Type.GetType(_connectorDirectory + refName + _connectorSuffix);
            if (connectorType == null) throw new InvalidConnectorException($"Invalid connector name. Found no connector associated with name {refName}");
            return (ObjectType) Activator.CreateInstance(connectorType);
        }
    }
}
