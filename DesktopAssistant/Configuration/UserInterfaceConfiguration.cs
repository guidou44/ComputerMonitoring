using Common.UI.Infrastructure;
using DesktopAssistant.UI;

namespace DesktopAssistant.Configuration
{
    public class UserInterfaceConfiguration : NotifyPropertyChanged, IUiSettings
    {
        private string _backGroundMain;
        private double _backGroundMainOpacity;
        private string _backGroundSecondary;
        private double _backGroundSecondaryOpacity;
        private string _progressBar;
        private string _progressBarBackGround;
        private string _fontMain;
        private string _main;

        public string Main
        {
            get => _main;
            set
            {
                _main = value;
                RaisePropertyChanged(nameof(Main));
            }
        }
        
        public string ProgressBar
        {
            get => _progressBar;
            set
            {
                _progressBar = value;
                RaisePropertyChanged(nameof(ProgressBar));
            }
        }

        public string ProgressBarBackGround
        {
            get => _progressBarBackGround;
            set
            {
                _progressBarBackGround = value;
                RaisePropertyChanged(nameof(ProgressBarBackGround));
            }
        }

        public string FontMain
        {
            get => _fontMain;
            set
            {
                _fontMain = value;
                RaisePropertyChanged(nameof(FontMain));
            }
        }

        public string BackGroundMain
        {
            get => _backGroundMain;
            set
            {
                _backGroundMain = value;
                RaisePropertyChanged(nameof(BackGroundMain));
            }
        }

        public string BackGroundSecondary
        {
            get => _backGroundSecondary;
            set
            {
                _backGroundSecondary = value;
                RaisePropertyChanged(nameof(BackGroundSecondary));
            }
        }

        public double BackGroundMainOpacity
        {
            get => _backGroundMainOpacity;
            set
            {
                _backGroundMainOpacity = value;
                RaisePropertyChanged(nameof(BackGroundMainOpacity));
            }
        }

        public double BackGroundSecondaryOpacity
        {
            get => _backGroundSecondaryOpacity;
            set
            {
                _backGroundSecondaryOpacity = value;
                RaisePropertyChanged(nameof(BackGroundSecondaryOpacity));
            }
        }
    }
}