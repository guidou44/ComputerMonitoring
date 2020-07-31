using System.Windows;
using Common.UI.Interfaces;
using Common.UI.WindowProperty;

namespace DesktopAssistant.Views
{
    public partial class ProcessWatchSettingsView : Window, IDialog, IDragable
    {
        public ProcessWatchSettingsView()
        {
            InitializeComponent();
        }
    }
}