using Common.UI.Interfaces;
using DesktopAssistant.Tests.Common.UI.Tests.Interfaces.Tests.Tests.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DesktopAssistant.Tests.Common.UI.Tests.Interfaces.Tests.Exceptions;

namespace DesktopAssistant.Tests.Common.UI.Tests.Interfaces.Tests.Fixtures
{
    public class DialogViewWithDataContextFixture : DialogViewFixture
    {
        public override bool? ShowDialog()
        {
            throw new ErrorOrMessageDialogShownException(this.DataContext);
        }
    }
}
