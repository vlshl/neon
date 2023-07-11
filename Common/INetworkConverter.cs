using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface INetworkConverter
    {
        string Convert(double[] arr);
        double[] ConvertBack(string label);
    }
}
