using Common.UI.DialogServices;
using Common.UI.Interfacea;
using Common.UI.WindowProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistant.Tests.Common.UI.Tests.Interfaces.Tests.Fixtures
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
