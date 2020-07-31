using Common.UI.WindowProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopAssistant.Tests.Common.UI.Tests.ViewModel.Tests.Events;

namespace DesktopAssistant.Tests.Common.UI.Tests.ViewModel.Tests.Fixtures
{
    public class Dragable : IDragable
    {
        public delegate void EventHandler(DragActionExecutedEventArgs e);
        public event EventHandler DragActionEvent;
        public void DragMove()
        {
            DragActionEvent?.Invoke(new DragActionExecutedEventArgs(this.GetHashCode().ToString()));
        }
    }
}
