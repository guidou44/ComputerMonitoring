using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Models
{
    public class GpuTemp : HardwdareInformation
    {
        public GpuTemp()
        {
            ShortName = "GPU";
        }

        public override string ToString()
        {
            return Main_Value.ToString() + " °C";
        }
    }
}
