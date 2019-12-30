using HardwareManipulation.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Factories
{
    public class ConnectorFactory
    {
        public ConnectorBase InstantiateConnector(string connectorName)
        {
            var connectors = typeof(ConnectorBase)
            .Assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ConnectorBase)) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);
            var connector = connectors.Where(TYPE => TYPE.Name == connectorName + "_Connector" || TYPE.Name == connectorName + "Connector");
            if (connector.Count() != 1) throw new ArgumentException($"Invalid connector name. Found {connector.Count()} connector associated with this name.");
            return (ConnectorBase) Activator.CreateInstance(connector.Single());
        }
    }
}
