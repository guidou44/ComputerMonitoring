using System;
using Hardware.Connectors;
using Hardware.Exceptions;

namespace Hardware.Factories
{
    public class ConnectorFactory : IFactory<ConnectorBase>
    {
        private static string _connectorDirectory = "Hardware.Connectors.";
        private static string _connectorSuffix = "_Connector";

        private Func<Type, ConnectorBase> delegateCreatorFunc;

        public ConnectorFactory(Func<Type, ConnectorBase> func) 
        {
            delegateCreatorFunc = func;
        }

        public ConnectorBase CreateInstance(string refName)
        {
            string assemblyName = typeof(IFactory<ConnectorBase>).Assembly.FullName;

            Type connectorType = Type.GetType(_connectorDirectory + refName + _connectorSuffix + $", {assemblyName}");
            if (connectorType == null) throw new InvalidConnectorException($"Invalid connector name. Found no connector associated with name {refName}");
            return delegateCreatorFunc(connectorType);
        }
    }
}