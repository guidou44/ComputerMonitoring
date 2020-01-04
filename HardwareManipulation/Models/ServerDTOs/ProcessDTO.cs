using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareAccess.ServerDTOs.Models
{
    public class ProcessDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int PID { get; set; }
        public int? Server_User_FK { get; set; }
    }
}
