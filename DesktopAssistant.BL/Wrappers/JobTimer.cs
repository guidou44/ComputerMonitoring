using System.Timers;

namespace DesktopAssistant.BL.Wrappers
{
    public class JobTimer : ITimer
    {
        private Timer _timerInternal;

        public JobTimer()
        {
            _timerInternal = new Timer();
        }

        public bool AutoReset 
        {
            set => _timerInternal.AutoReset = value;
        }
        
        public bool Enabled 
        {
            set => _timerInternal.Enabled = value;
        }

        public event ElapsedEventHandler Elapsed
        {
            add => _timerInternal.Elapsed += value;
            remove => _timerInternal.Elapsed -= value;
        }

        public void Init(int milliseconds)
        {
            _timerInternal = new Timer(milliseconds);
        }

        public void Start()
        {
            _timerInternal?.Start();
        }

        public void Stop()
        {
            _timerInternal?.Stop();
        }
    }
}