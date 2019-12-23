using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.UI.Interfaces;
using Common.UI.ViewModels;

namespace WPF_DS_Template.ViewModels
{
    public class MainViewModel : WindowViewModelBase
    {
        #region Constructor

        public MainViewModel(IDialogService dialogService) : base(dialogService)
        {
           
        }

        #endregion

        #region Methods

        private void SubscribeToEvents()
        {

        }

        #endregion

        #region Properties

        #endregion

        #region Commands

        #endregion
    }
}
