using Common.UI.DialogServices;
using Common.UI.Interfaces;
using Common.UI.WindowProperty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComputerMonitoringTests.Common.UI.Tests.Interfaces.Fixtures
{
    public class DialogViewInvalidFixture : IDialog
    {
        public object DataContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool? DialogResult { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Window Owner { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public bool? ShowDialog()
        {
            throw new NotImplementedException();
        }
    }
}
