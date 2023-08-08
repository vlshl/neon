using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface INeuronetConverter
    {
        string Convert(double[] arr);
        double[] ConvertBack(string label);
    }
}
