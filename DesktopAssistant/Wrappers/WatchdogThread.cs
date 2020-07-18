using DesktopAssistant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesktopAssistant.Wrappers
{
    public class WatchdogThread : IThread
    {
        private Thread _watchdogThreadInternal;
        public void Abort()
        {
            _watchdogThreadInternal?.Abort();
        }

        public void SetJob(Action jobAction)
        {
            _watchdogThreadInternal = new Thread(() => jobAction.Invoke());
        }

        public void Start()
        {
            _watchdogThreadInternal?.Start();
        }
    }
}
