using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.ServerDTOs.Models
{
    public class ServerResourceDTO
    {
        public int Id { get; set; }
        public double Value { get; set; }

        public int Sample_Time_FK { get; set; }
        public SampleTimeDTO Sample_Time { get; set; }
        public int Server_Resource_Unit_FK { get; set; }
        public virtual ServerResourceUnitDTO Server_Resource_Unit { get; set; }
        public int? Process_FK { get; set; }
        public virtual ProcessDTO Process { get; set; }
        public int? Resource_Type_FK { get; set; }
        public virtual ResourceTypeDTO Resource_Type { get; set; }
    }
}
