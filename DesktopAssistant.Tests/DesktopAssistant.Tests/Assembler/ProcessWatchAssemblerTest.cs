using DesktopAssistant.Assembler;
using DesktopAssistant.BL.ProcessWatch;
using DesktopAssistant.ViewModels;
using Moq;
using Xunit;

namespace DesktopAssistant.Tests.DesktopAssistant.Tests.Assembler
{
    public class ProcessWatchAssemblerTest
    {
        private const string ProcessName = "Test";
        
        [Fact]
        public void GivenModel_WhenAssemble_ThenItAssembleToViewModel()
        {
            Mock<IProcessWatch> model = new Mock<IProcessWatch>();
            model.SetupGet(m => m.IsRunning).Returns(true);
            model.SetupGet(m => m.ProcessName).Returns(ProcessName);
            model.SetupGet(m => m.DoCapture).Returns(true);
            
            ProcessViewModel vm = ProcessWatchAssembler.AssembleFromModel(model.Object);
            
            Assert.NotNull(vm);
            Assert.Equal(ProcessName, vm.ProcessName);
            Assert.True(vm.IsRunning);
            Assert.True(vm.DoCapture);
        }

        [Fact]
        public void GivenViewModel_WhenAssemble_ThenItAssembleToModel()
        {
            Mock<ProcessViewModel> viewModel = new Mock<ProcessViewModel>(true, "someName");
            viewModel.SetupGet(m => m.IsRunning).Returns(true);
            viewModel.SetupGet(m => m.ProcessName).Returns(ProcessName);
            viewModel.SetupGet(m => m.DoCapture).Returns(true);
            
            IProcessWatch model = ProcessWatchAssembler.AssembleFromViewModel(viewModel.Object);
            
            Assert.NotNull(model);
            Assert.Equal(ProcessName, model.ProcessName);
            Assert.False(model.IsRunning);//Process is null
            Assert.True(model.DoCapture);
        }
    }
}