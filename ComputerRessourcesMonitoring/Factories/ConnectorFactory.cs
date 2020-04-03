using Autofac;
using HardwareAccess.Connectors;
using HardwareManipulation;
using HardwareManipulation.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerRessourcesMonitoring.Factories
{
    public class ConnectorFactory : IFactory<ConnectorBase>
    {
        private static string _connectorDirectory = "HardwareAccess.Connectors.";
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
