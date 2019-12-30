using Common.Helpers;
using Common.UI.Infrastructure;
using Common.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Testing_Common_UI.ViewModels
{
    public class MainViewModel : WindowViewModelBase
    {
        public MainViewModel()
        {
        }




        public ICommand TestErrorMessageCommand
        {
            get { return new RelayCommand(TestErrorMessageCommandExecute); }
        }

        public void TestErrorMessageCommandExecute()
        {
            try
            {
                throw new ArgumentNullException("This is my exception");
            }
            catch (Exception e)
            {

                _dialogService.ShowException(e);
            }
            
        }

        public ICommand TestMessageCommand
        {
            get { return new RelayCommand(TestMessageCommandExecute); }
        }

        public void TestMessageCommandExecute()
        {
            _dialogService.ShowMessageBox("Message is here");
        }

    }
}
