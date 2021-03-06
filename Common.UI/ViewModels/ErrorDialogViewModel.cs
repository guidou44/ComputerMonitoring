﻿using Common.UI.DialogServices;
using Common.UI.Infrastructure;
using Common.UI.Interfaces;
using Common.UI.WindowProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Common.UI.ViewModels
{
    public class ErrorDialogViewModel : DialogViewModelBase
    {
        protected Exception _currentException;

        public ErrorDialogViewModel(Exception e)
        {
            _currentException = e;
            ErrorMessage = e.Message + "\n\n" + $"StackTrace: {e.StackTrace}";
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set 
            { 
                _errorMessage = value;
                RaisePropertyChanged(nameof(ErrorMessage));
            }
        }

    }
}
