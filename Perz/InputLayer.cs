using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perz
{
    public class InputLayer : ILayer
    {
        private int _size;
        private double[] _arr;

        public InputLayer(int size)
        {
            _size = size;
            _arr = new double[size];
        }

        public double[] Outputs { get { return _arr; } }

        public double[] Inputs { get { return _arr; } set { _arr = value; } }

        public int Size { get { return _size; } }
    }
}
