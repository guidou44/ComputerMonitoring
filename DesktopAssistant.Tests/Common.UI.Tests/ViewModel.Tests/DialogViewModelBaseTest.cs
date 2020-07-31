using Common.UI.DialogServices;
using DesktopAssistant.Tests.Common.UI.Tests.ViewModel.Tests.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopAssistant.Tests.Common.UI.Tests.ViewModel.Tests.Fixtures;
using Xunit;

namespace DesktopAssistant.Tests.Common.UI.Tests.ViewModel.Tests
{
    public class DialogViewModelBaseTest
    {
        private string TEST_OUTPUT;

        [Fact]
        public async void GivenDragableEntity_WhenINvokingDragCommand_ThenItRaisesEvent()
        {
            DialogViewModelBaseFixture dialogVmSubject = new DialogViewModelBaseFixture();
            Dragable dragable = new Dragable();
            string response = null;

            dragable.DragActionEvent += e => response = e.Message;
            await Task.Run(() => dragable.DragMove());


            Assert.Equal(dragable.GetHashCode().ToString(), response);
            dragable.DragActionEvent -= e => response = e.Message;
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
