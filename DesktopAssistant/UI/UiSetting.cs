using System.Windows.Media;
using Common.UI.Infrastructure;

namespace DesktopAssistant.UI
{
    public class UiSetting : NotifyPropertyChanged, IUiSettings
    {
        private object _background;
        private object _borderBrush;
        private object _foreground;

        public UiSetting()
        {
            BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#32a852"));
            Foreground = BorderBrush;
            Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00FFFFFF"));
        }

        public object BorderBrush
        {
            get { return _borderBrush; }
            set 
            { 
                _borderBrush = value;
                RaisePropertyChanged(nameof(BorderBrush));
            }
        }

        public object Background
        {
            get { return _background; }
            set
            {
                _background = value;
                RaisePropertyChanged(nameof(Background));
            }
        }

        public object Foreground
        {
            get { return _foreground; }
            set
            {
                _foreground = value;
                RaisePropertyChanged(nameof(Foreground));
            }
        }
    }
}