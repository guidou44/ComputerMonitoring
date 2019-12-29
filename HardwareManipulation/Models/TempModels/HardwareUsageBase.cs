using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Models
{
    public abstract class HardwareUsageBase
    {
        public string ShortName { get; set; }
        public string Name { get; set; }
        public object Main_Value { get; set; }
    }
}
