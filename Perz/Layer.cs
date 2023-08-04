using Common;

namespace Perz
{
    public abstract class Layer : ILayer
    {
        protected ILayer prevLayer;
        protected Layer? nextLayer;
        protected int size;
        protected double[] outputs;
        protected double[,] weights;
        protected double[,] weight_deltas;
        protected double[] deltas;
        protected double train_alpha;
        protected double train_nju;

        public Layer(ILayer prevLayer, int size)
        {
            this.prevLayer = prevLayer;
            this.size = size;
            outputs = new double[size];
            weights = new double[size, prevLayer.Size];
            weight_deltas = new double[size, prevLayer.Size];
            deltas = new double[size];
            nextLayer = null;

            train_alpha = 0.1;
            train_nju = 0.1;
        }

        public void SetNextLayer(Layer layer)
        {
            nextLayer = layer;
        }

        public double[] Outputs { get { return outputs; } }

        public double[,] Weights { get { return weights; } }

        public int Size { get { return size; } }

        public double[] Deltas { get { return deltas; } }

        public void Execute()
        {
            for (int i = 0; i < size; ++i)
            {
                double sum = 0;
                for (int j = 0; j < prevLayer.Size; ++j)
                {
                    sum += prevLayer.Outputs[j] * weights[i, j];
                }
                //sum += weights[i, 0];
                outputs[i] = 1 / (1 + Math.Exp(-sum));
            }
        }

        public void LoadWeights(double[,] ws)
        {
            weights = ws;
        }

        public void InitWeights(InitWeightsMode mode)
        {
            var r = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < weights.GetLength(0); ++i)
            {
                for (int j = 0; j < weights.GetLength(1); ++j)
                {
                    if (mode == InitWeightsMode.Random)
                    {
                        weights[i, j] = r.NextDouble() * 2 - 1;
                    }
                    else if (mode == InitWeightsMode.Zero)
                    {
                        weights[i, j] = 0;
                    }
                    else
                    {
                        weights[i, j] = 0;
                    }
                }
            }
        }
    }
}
