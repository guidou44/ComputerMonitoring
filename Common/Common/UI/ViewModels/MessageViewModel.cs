using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UI.ViewModels
{
    public class MessageViewModel : DialogViewModelBase
    {
        public MessageViewModel(string message)
        {
            Message = message;
        }

        #region Properties

        private string _message;

        public string Message
        {
            get { return _message; }
            set 
            { 
                _message = value;
                RaisePropertyChanged(nameof(Message));
            }
        }


        #endregion

    }
}
