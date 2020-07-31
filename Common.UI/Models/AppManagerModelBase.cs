using Common.UI.Infrastructure;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UI.Models
{
    public abstract class AppManagerModelBase
    {
        protected IEventAggregator EventHub;

        public AppManagerModelBase(IEventAggregator eventHub)
        {
            EventHub = eventHub;
        }
    }
}
