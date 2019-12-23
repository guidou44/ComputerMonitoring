using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UI.Interfaces
{
    public interface IDispatcherService
    {
        bool IsSynchronized { get; }
        void BeginInvoke(Action action);
        void Invoke(Action action);
    }
}
