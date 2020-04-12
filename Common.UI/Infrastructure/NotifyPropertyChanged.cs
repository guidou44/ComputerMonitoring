using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UI.Infrastructure
{
   
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        public virtual event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
    }

}
