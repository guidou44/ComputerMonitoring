﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistant.Tests.Common.UI.Tests.Interfaces.Tests.Exceptions
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
