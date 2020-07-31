using Common.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DesktopAssistant.Tests.Common.UI.Tests.ViewModel.Tests
{
    public class ErrorDialogViewModelTest
    {
        [Fact]
        public void GivenException_WhenInstantiateErrorVm_ThenItFormatsMessageProperly()
        {
            const string exMessage = "@#$%^&111133_7645";
            Exception ex = new Exception(exMessage);
            
            ErrorDialogViewModel erroVmSubject = new ErrorDialogViewModel(ex);

            Assert.Equal(exMessage + "\n\n" + $"StackTrace: {ex.StackTrace}", erroVmSubject.ErrorMessage);
        }
    }
}
