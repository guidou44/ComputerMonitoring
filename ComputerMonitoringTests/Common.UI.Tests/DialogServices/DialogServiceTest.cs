using Common.UI.Interfaces;
using Common.UI.DialogServices;
using ComputerMonitoringTests.Common.UI.Tests.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerMonitoringTests.Common.UI.Tests.DialogServices
{
    public class DialogServiceTest : IDialogServiceTest
    {
        protected override IDialogService ProvideDialogService()
        {
            return new DialogService();
        }
    }
}
