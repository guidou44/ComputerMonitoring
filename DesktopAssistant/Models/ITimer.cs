using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DesktopAssistant.Models
{
    public interface ITimer
    {
        void Init(int milliseconds);
        void Start();
        void Stop();
        bool AutoReset { set; }
        event ElapsedEventHandler Elapsed;
    }
}
