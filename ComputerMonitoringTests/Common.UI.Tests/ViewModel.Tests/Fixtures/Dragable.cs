using Common.UI.WindowProperty;
using ComputerMonitoringTests.Common.UI.Tests.ViewModel.Tests.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerMonitoringTests.Common.UI.Tests.ViewModel.Tests.Fixtures
{
    public class Dragable : IDragable
    {
        public void DragMove()
        {
            throw new DragActionExecutedException();
        }
    }
}
