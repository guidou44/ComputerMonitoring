using Common.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Common.UI.DispatcherServices
{
    public sealed class DispatcherService : IDispatcherService
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherService() : this(Dispatcher.CurrentDispatcher){}

        public DispatcherService(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public bool IsSynchronized
        {
            get { return this._dispatcher.Thread == Thread.CurrentThread;}
        }


        public void BeginInvoke(Action action)
        {
            _dispatcher.BeginInvoke(action);
        }

        public void Invoke(Action action)
        {
            _dispatcher.Invoke(action);
        }
    }
}
