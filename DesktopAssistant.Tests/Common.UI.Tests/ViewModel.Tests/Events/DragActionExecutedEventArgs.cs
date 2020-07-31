using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistant.Tests.Common.UI.Tests.ViewModel.Tests.Events
{
    public class DragActionExecutedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public DragActionExecutedEventArgs(string message)
        {
            Message = message;
        }
    }
}
