using Common.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistant.Tests.Common.UI.Tests.ViewModel.Tests
{
    public class MessageDialogViewModelTest
    {
        [Fact]
        public void GivenMessage_WhenInstantiateMessageVm_ThenItFormatsMessageProperly()
        {
            const string message = "@#$%^&111133_7645";

            MessageDialogViewModel messageVmSubject = new MessageDialogViewModel(message);

            Assert.Equal(message, messageVmSubject.Message);
        }
    }
}
