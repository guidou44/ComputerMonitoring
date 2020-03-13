using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.UI.Interfaces
{
    public interface IRelocatable
    {
        double ActualHeight { get;}
        double ActualWidth { get;}
        double Left { get; set; }
        double Top { get; set; }
    }
}
