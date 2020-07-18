using Common.UI.DialogServices;
using Common.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistantTests.Common.UI.Tests.Interfaces.Fixtures
{
    public class ErrorDialogViewModelFixture : ErrorDialogViewModel
    {
        public ErrorDialogViewModelFixture(Exception e) : base(e) { }
    }
}
