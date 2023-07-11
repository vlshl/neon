using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Perz
{
    public class HiddenLayer : Layer
    {
        public HiddenLayer(ILayer prevLayer, int size) : base(prevLayer, size)
        {
        }

        public void Train()
        {
            for (int i = 0; i < size; ++i)
            {
                double sum = 0;
                for (int j = 0; j < nextLayer.Size; ++j)
                {
                    sum += nextLayer.Deltas[j] * nextLayer.Weights[j, i];
                }
                deltas[i] = Outputs[i] * (1 - Outputs[i]) * sum;
            }

            for (int i = 0; i < weights.GetLength(0); ++i)
                for (int j = 0; j < weights.GetLength(1); ++j)
                {
                    weight_deltas[i, j] = train_alpha * weight_deltas[i, j] + (1 - train_alpha) * train_nju * deltas[i] * prevLayer.Outputs[j];
                    weights[i, j] = weights[i, j] - weight_deltas[i, j];
                }
        }
    }
}
