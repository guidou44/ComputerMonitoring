using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Common.UI.Interfaces;
using Common.UI.WindowProperty;

namespace Common.UI.Views
{
    /// <summary>
    /// Interaction logic for ErrorMessageView.xaml
    /// </summary>
    public partial class ErrorMessageView : Window, IDialog
    {
        public ErrorMessageView()
        {
            InitializeComponent();
        }
    }
}
