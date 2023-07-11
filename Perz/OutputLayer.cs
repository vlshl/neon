using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perz
{
    public class OutputLayer : Layer
    {
        public OutputLayer(ILayer inLayer, int size) : base(inLayer, size)
        {
        }

        public void Train(double[] targets)
        {
            for (int i = 0; i < size; ++i)
            {
                deltas[i] = -outputs[i] * (1 - outputs[i]) * (targets[i] - outputs[i]);
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
