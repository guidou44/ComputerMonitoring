using DesktopAssistant.Assembler;
using DesktopAssistant.BL.Hardware;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.ViewModels;
using Moq;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.Tests.Assembler
{
    public class HardwareAssemblerTest
    {
        private const string ShortName = "TestSn";
        private const string Unit = "TestUnit";
        private const double MainValue = 5.2;
        
        [Fact]
        public void GivenModel_WhenAssemble_ThenItAssembleToViewModel()
        {
            Mock<IHardwareInfo> model = new Mock<IHardwareInfo>();
            model.SetupGet(m => m.ShortName).Returns(ShortName);
            model.SetupGet(m => m.UnitSymbol).Returns(Unit);
            model.SetupGet(m => m.MainValue).Returns(MainValue);
            
            HardwareViewModel vm = HardwareAssembler.AssembleFromModel(model.Object);
            
            Assert.NotNull(vm);
            Assert.Equal(ShortName, vm.ShortName);
            Assert.Equal(Unit, vm.UnitSymbol);
            Assert.Equal(MainValue, vm.MainValue);
        }

        [Fact]
        public void GivenViewModel_WhenAssemble_ThenItAssembleToModel()
        {
            Mock<HardwareViewModel> viewModel = new Mock<HardwareViewModel>();
            viewModel.SetupGet(m => m.ShortName).Returns(ShortName);
            viewModel.SetupGet(m => m.UnitSymbol).Returns(Unit);
            viewModel.SetupGet(m => m.MainValue).Returns(MainValue);
            
            IHardwareInfo model = HardwareAssembler.AssembleFromViewModel(viewModel.Object);
            
            Assert.NotNull(model);
            Assert.Equal(ShortName, model.ShortName);
            Assert.Equal(Unit, model.UnitSymbol);
            Assert.Equal(MainValue, model.MainValue);
        }
    }
}