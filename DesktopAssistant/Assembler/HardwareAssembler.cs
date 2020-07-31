using DesktopAssistant.BL.Hardware;
using DesktopAssistant.ViewModels;
using Hardware.Models;

namespace DesktopAssistant.Assembler
{
    public static class HardwareAssembler
    {
        public static HardwareViewModel AssembleFromModel(IHardwareInfo model)
        {
            return new HardwareViewModel()
            {
                ShortName = model.ShortName,
                UnitSymbol = model.UnitSymbol,
                MainValue = model.MainValue
            };

        }
        
        public static IHardwareInfo AssembleFromViewModel(HardwareViewModel viewModel)
        {
            return new HardwareInformation()
            {
                ShortName = viewModel.ShortName,
                UnitSymbol = viewModel.UnitSymbol,
                MainValue = viewModel.MainValue
            };
        }
    }
}