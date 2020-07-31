namespace DesktopAssistant.ViewModels
{
    public class HardwareViewModel
    {
        public virtual object MainValue { get; set; }
        public virtual string ShortName { get; set; }
        public virtual string UnitSymbol { get; set; }
        
        public override string ToString()
        {
            return $"{MainValue} {UnitSymbol}";
        }
    }
}