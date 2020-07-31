using DesktopAssistant.BL.Hardware;
using Hardware.Models;

namespace Hardware.Connectors
{
    public abstract class ConnectorBase
    {
        public abstract HardwareInformation GetValue(MonitoringTarget resource);
    }
}