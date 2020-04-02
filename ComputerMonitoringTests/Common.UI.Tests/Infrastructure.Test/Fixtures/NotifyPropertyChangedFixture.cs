using Common.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerMonitoringTests.Common.UI.Tests.Infrastructure.Test.Fixtures
{
    public class NotifyPropertyChangedFixture : NotifyPropertyChanged
    {
        private string _modifiedVariable = String.Empty;

        public string ModifiedVariable
        {
            get { return _modifiedVariable; }
            set 
            {
                _modifiedVariable = value;
                RaisePropertyChanged("1");
                RaisePropertyChanged("2");
                RaisePropertyChanged("3");
            }
        }

    }
}
