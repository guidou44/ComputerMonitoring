using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DesktopAssistant.BL
{
    public interface ITimer
    {
        void Init(int milliseconds);
        void Start();
        void Stop();
        bool AutoReset { set; }
        bool Enabled { set; }
        event ElapsedEventHandler Elapsed;
    }
}
