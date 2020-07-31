namespace DesktopAssistant.BL.Hardware
{
    public interface IHardwareInfo
    {
        object MainValue { get; set; }
        string ShortName { get; set; }
        string UnitSymbol { get; set; }
    }
}