using Common.UI.DialogServices;
using ComputerMonitoringTests.Common.UI.Tests.ViewModel.Tests.Exceptions;
using ComputerMonitoringTests.Common.UI.Tests.ViewModel.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ComputerMonitoringTests.Common.UI.Tests.ViewModel.Tests
{
    public class DialogViewModelBaseTest
    {
        private string TEST_OUTPUT;

        [Fact]
        public void GivenDragableEntity_WhenINvokingDragCommand_ThenItExecuteAction()
        {
            DialogViewModelBaseFixture dialogVmSubject = new DialogViewModelBaseFixture();
            Dragable dragable = new Dragable();

            Assert.Throws<DragActionExecutedException>(() => dialogVmSubject.DragWindowCommand.Execute(dragable));
        }

        [Fact]
        public void GivenRegisteredEventHandler_WhenExecuteOkCommand_ThenEventIsProperlyRaised()
        {
            DialogViewModelBaseFixture dialogVmSubject = new DialogViewModelBaseFixture();
            dialogVmSubject.CloseRequested += ChangeTestOutput;

            dialogVmSubject.OkCommand.Execute(null);
            dialogVmSubject.CloseRequested -= ChangeTestOutput;

            Assert.Equal("Y", TEST_OUTPUT);
        }

        [Fact]
        public void GivenRegisteredEventHandler_WhenExecuteCancelCommand_ThenEventIsProperlyRaised()
        {
            DialogViewModelBaseFixture dialogVmSubject = new DialogViewModelBaseFixture();
            dialogVmSubject.CloseRequested += ChangeTestOutput;

            dialogVmSubject.CancelCommand.Execute(null);
            dialogVmSubject.CloseRequested -= ChangeTestOutput;

            Assert.Equal("N", TEST_OUTPUT);
        }

        [Fact]
        public void GivenRegisteredEventHandler_WhenExecuteMultipleCommands_ThenEventsAreProperlyRaised()
        {
            DialogViewModelBaseFixture dialogVmSubject = new DialogViewModelBaseFixture();
            dialogVmSubject.CloseRequested += ChangeTestOutput;

            dialogVmSubject.CancelCommand.Execute(null);
            dialogVmSubject.CancelCommand.Execute(null);
            dialogVmSubject.OkCommand.Execute(null);
            dialogVmSubject.CancelCommand.Execute(null);
            dialogVmSubject.OkCommand.Execute(null);
            dialogVmSubject.CloseRequested -= ChangeTestOutput;

            Assert.Equal("NNYNY", TEST_OUTPUT);
        }

        private void ChangeTestOutput(object sender, DialogCloseRequestedEventArgs e)
        {
            if ((bool) e.DialogResult)
                TEST_OUTPUT += "Y";
            else
                TEST_OUTPUT += "N";
        }

    }
}
