using Common.UI.WindowProperty;
using Common.UI.DialogServices;
using DesktopAssistantTests.Common.UI.Tests.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistantTests.Common.UI.Tests.DialogServices
{
    public class DialogServiceTest : IDialogServiceTest
    {
        protected override IDialogService ProvideDialogService()
        {
            return new DialogService(null);
        }
    }
}
