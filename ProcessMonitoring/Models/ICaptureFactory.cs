using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMonitoring.Models
{
    public interface ICaptureFactory<T>
    {
        T CreateInstance(string reference);
    }
}
