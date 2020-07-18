using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware
{
    public interface IFactory<ObjectType>
    {
        ObjectType CreateInstance(string refName);
    }
}
