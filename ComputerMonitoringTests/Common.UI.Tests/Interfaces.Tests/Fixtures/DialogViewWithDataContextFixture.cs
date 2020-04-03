using Common.UI.Interfaces;
using ComputerMonitoringTests.Common.UI.Tests.Interfaces.Exceptions;
using ComputerMonitoringTests.Common.UI.Tests.Interfaces.Tests.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComputerMonitoringTests.Common.UI.Tests.Interfaces.Fixtures
{
    public class DialogViewWithDataContextFixture : DialogViewFixture
    {
        public override bool? ShowDialog()
        {
            throw new ErrorOrMessageDialogShownException(this.DataContext);
        }
    }
}
