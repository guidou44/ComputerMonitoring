using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerMonitoringTests.Common.UI.Tests.Interfaces.Exceptions
{
    public class ErrorOrMessageDialogShownException : Exception
    {
        public object DataContext;
        public ErrorOrMessageDialogShownException(object dataContext) : base(dataContext.GetType().Name) 
        {
            DataContext = dataContext;
        }
    }
}
