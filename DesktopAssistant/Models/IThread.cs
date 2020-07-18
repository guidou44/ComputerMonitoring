using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopAssistant.Models
{
    public interface IThread
    {
        void SetJob(Action jobAction);
        void Start();
        void Abort();
    }
}
