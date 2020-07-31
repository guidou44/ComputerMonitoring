using Common.UI.WindowProperty;
using Common.UI.DialogServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopAssistant.Tests.Common.UI.Tests.Interfaces.Tests;

namespace DesktopAssistant.Tests.Common.UI.Tests.DialogServices
{
    public class DialogServiceTest : IDialogServiceTest
    {
        protected override IDialogService ProvideDialogService()
        {
            return new DialogService(null);
        }
    }
}
