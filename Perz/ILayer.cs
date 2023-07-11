using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perz
{
    public interface ILayer
    {
        double[] Outputs { get; }
        int Size { get; }
    }
}
