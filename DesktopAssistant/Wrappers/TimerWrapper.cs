using DesktopAssistant.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DesktopAssistant.Wrappers
{
    public class TimerWrapper : ITimer
    {
        private System.Timers.Timer _timerInternal;

        public bool AutoReset 
        {
            set 
            { 
                _timerInternal.AutoReset = true;
                _timerInternal.Enabled = true;
            }
        }

        public event ElapsedEventHandler Elapsed
        {
            add { _timerInternal.Elapsed += value; }
            remove { _timerInternal.Elapsed -= value; }
        
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
            _timerInternal?.Dispose();
        }
    }
}
