using Common.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistantTests.Common.UI.Tests.Interfaces.Fixtures
{
    public class MessageDialogViewModelFixture : MessageDialogViewModel
    {
        public MessageDialogViewModelFixture(string message) : base(message) { }

        public void RequestCloseWithCancel()
        {
            CancelCommand.Execute(null);
        }
    }
}
