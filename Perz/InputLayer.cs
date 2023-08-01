using Newtonsoft.Json.Linq;
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
            for (int i = 0; i < size; ++i)
            {
                _arr[i] = 0;
            }
        }

        public double[] Outputs { get { return _arr; } }

        public double[] Inputs 
        { 
            get 
            { 
                return _arr; 
            } 
            set 
            { 
                for (int i = 0; i < _size; ++i) 
                {
                    if (i < value.Length)
                    {
                        _arr[i] = value[i];
                    }
                    else
                    {
                        _arr[i] = 0;
                    }
                
                }
            } 
        }

        public int Size { get { return _size; } }
    }
}
