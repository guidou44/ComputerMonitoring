using Common.UI.DialogServices;
using Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Common.UI.Interfaces;
using Common.UI.Infrastructure;

namespace Common.Examples
{
    //USAGE


    public class MainWindowViewModelEXAMPLE
    {
        private readonly IDialogService dialogService;

        public MainWindowViewModelEXAMPLE(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            DisplayMessageCommand = new RelayCommand(DisplayMessage);
        }

        public ICommand DisplayMessageCommand { get; }

        private void DisplayMessage()
        {
            var viewModel = new DialogViewModelEXAMPLE("Hello!");

            bool? result = dialogService.ShowDialog(viewModel);

            if (result.HasValue)
            {
                if (result.Value)
                {
                    //Accepted
                    }
                else
                {
                    //Cancelled
                    }
            }
        }


        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    IDialogService dialogService = new DialogService(MainWindow);

        //    dialogService.Register<DialogViewModel, DialogWindow>();

        //    var viewModel = new MainWindowViewModel(dialogService);
        //    var view = new MainWindow { DataContext = viewModel };

        //    view.ShowDialog();
        //}
    }
}

