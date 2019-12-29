using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation.Models
{
    public class HardwdareInformation
    {
        public string ShortName { get; set; }

        public object Main_Value { get; set; }

        public override string ToString()
        {
            return Main_Value.ToString() + $" {UnitSymbol}";
        }

        public string UnitSymbol { get; set; }
    }
}
