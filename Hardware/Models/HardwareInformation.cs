using DesktopAssistant.BL.Hardware;

namespace Hardware.Models
{
    public class HardwareInformation : IHardwareInfo
    {
        public object MainValue { get; set; }
        public string ShortName { get; set; }
        public string UnitSymbol { get; set; }
        
    }
}