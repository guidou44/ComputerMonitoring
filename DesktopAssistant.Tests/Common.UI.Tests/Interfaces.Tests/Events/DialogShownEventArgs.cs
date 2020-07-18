using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistantTests.Common.UI.Tests.Interfaces.Tests.Events
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
