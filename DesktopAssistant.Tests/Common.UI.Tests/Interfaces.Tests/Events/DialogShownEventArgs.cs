using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistant.Tests.Common.UI.Tests.Interfaces.Tests.Tests.Events
{
    public class DialogShownEventArgs : EventArgs
    {
        public object DataContext { get; set; }
        public DialogShownEventArgs(object dataContext)
        {
            DataContext = dataContext;
        }
    }
}
