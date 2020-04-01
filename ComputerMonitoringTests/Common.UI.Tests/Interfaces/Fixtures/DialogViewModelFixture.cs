using Common.UI.DialogServices;
using Common.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerMonitoringTests.Common.UI.Tests.Interfaces.Fixtures
{
    public class DialogViewModelFixture : IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public void RequestCloseWithOk()
        {
            CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true));
        }

        public void RequestCloseWithCancel()
        {
            CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(false));
        }

        public void RequestCloseWithX()
        {
            CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(null));
        }
    }
}
