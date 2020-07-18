using Common.UI.Interfaces;
using Common.UI.WindowProperty;
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

namespace DesktopAssistant.Views
{
    /// <summary>
    /// Interaction logic for SettingsDialogView.xaml
    /// </summary>
    public partial class SettingsDialogView : Window, IDialog, IDragable
    {
        public SettingsDialogView()
        {
            InitializeComponent();
        }
    }
}
