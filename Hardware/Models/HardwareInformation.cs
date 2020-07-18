using Hardware.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Models
{
    public class HardwareInformation
    {
        public object MainValue { get; set; }
        public string ShortName { get; set; }
        public string UnitSymbol { get; set; }
        public override string ToString()
        {
            return $"{MainValue} {UnitSymbol}";
        }
    }
}
