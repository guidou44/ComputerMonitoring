using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareManipulation
{
    public interface IFactory
    {
        ObjectType CreateInstance<ObjectType>(string refName);
    }
}
