using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perz
{
    public class PerzConverter : INetworkConverter
    {
        public string Convert(double[] outputs)
        {
            if ((outputs == null) || (outputs.Length == 0)) return "";

            double max = outputs[0];
            int res = 0;
            for (int i = 1; i < outputs.Length; ++i)
            {
                double d = outputs[i];
                if (d > max)
                {
                    res = i;
                    max = d;
                }
            }

            return res.ToString();
        }

        public double[] ConvertBack(string label)
        {
            double[] arr = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            if (label == "0")
            {
                arr[0] = 1;
            }
            else if (label == "1")
            {
                arr[1] = 1;
            }
            else if (label == "2")
            {
                arr[2] = 1;
            }
            else if (label == "3")
            {
                arr[3] = 1;
            }
            else if (label == "4")
            {
                arr[4] = 1;
            }
            else if (label == "5")
            {
                arr[5] = 1;
            }
            else if (label == "6")
            {
                arr[6] = 1;
            }
            else if (label == "7")
            {
                arr[7] = 1;
            }
            else if (label == "8")
            {
                arr[8] = 1;
            }
            else if (label == "9")
            {
                arr[9] = 1;
            }

            return arr;
        }
    }
}
