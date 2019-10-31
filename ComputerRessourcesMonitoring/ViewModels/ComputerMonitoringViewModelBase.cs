using Common.UI.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ComputerRessourcesMonitoring.ViewModels
{
    public abstract class ComputerMonitoringViewModelBase : NotifyPropertyChanged
    {
        protected Timer _monitoringRefreshCounter;
        protected void SetMonitoringCounter(int counterTimeMilliseconds)
        {
            _monitoringRefreshCounter = new Timer(counterTimeMilliseconds);
            _monitoringRefreshCounter.Elapsed += OnCounterCompletionEvent;
            _monitoringRefreshCounter.AutoReset = true;
            _monitoringRefreshCounter.Enabled = true;
        }

        protected abstract void OnCounterCompletionEvent(Object source, ElapsedEventArgs e);
        protected abstract void RefreshMonitoring();
    }
}
