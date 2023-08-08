using Common;

namespace Perz
{
    public class PerzNeuronet : INeuronet
    {
        private InputLayer _inputLayer;
        private List<HiddenLayer> _hiddenLayers;
        private OutputLayer _outputLayer;
        private readonly PerzSettings _settings;

        public PerzNeuronet(PerzSettings settings)
        {
            _settings = settings;
            _hiddenLayers = new List<HiddenLayer>();
            ILayer prev;
            _inputLayer = new InputLayer(settings.InputSize);
            prev = _inputLayer;
            foreach (var size in settings.HiddenLayerSizes)
            {
                HiddenLayer h = new HiddenLayer(prev, size);
                prev = h;
                _hiddenLayers.Add(h);
            }

            if (_hiddenLayers.Any())
            {
                _outputLayer = new OutputLayer(_hiddenLayers.Last(), settings.OutputSize);

                for(int i = 0; i < _hiddenLayers.Count - 1; ++i)
                {
                    _hiddenLayers[i].SetNextLayer(_hiddenLayers[i + 1]);
                }
                _hiddenLayers[_hiddenLayers.Count - 1].SetNextLayer(_outputLayer);
            }
            else
            {
                _outputLayer = new OutputLayer(_inputLayer, settings.OutputSize);
            }
        }

        public double[] Outputs { get { return _outputLayer.Outputs; } }

        public string GetName()
        {
            return _settings.Name;
        }

        public object GetSettings()
        {
            return _settings;
        }

        public int GetHiddenLayersCount()
        {
            return _hiddenLayers.Count;
        }

        public IEnumerable<double[,]> GetAllWeights()
        {
            var weights = new List<double[,]>();
            weights.Add(_outputLayer.Weights);

            foreach (var layer in _hiddenLayers)
            {
                weights.Add(layer.Weights);
            }

            return weights;
        }

        /// <summary>
        /// Получить массий весов указанного слоя
        /// </summary>
        /// <param name="layer">Слой: 0-выходной, 1...-скрытые</param>
        /// <returns>Массив чисел. Если массив размером 0x0 - неверный аргумент</returns>
        public double[,] GetWeights(int layer = 0)
        {
            if (layer == 0)
            {
                return _outputLayer.Weights;
            }

            if (layer - 1 < _hiddenLayers.Count)
            {
                return _hiddenLayers[layer - 1].Weights;
            }

            return new double[0, 0];
        }

        public void LoadWeights(IEnumerable<double[,]> weights)
        {
            if (!weights.Any()) return;

            _outputLayer.LoadWeights(weights.First());
            int w_count = weights.Count();

            for (int i = 0; i < _hiddenLayers.Count; ++i)
            {
                if (i + 1 < w_count)
                {
                    _hiddenLayers[i].LoadWeights(weights.ElementAt(i + 1));
                }
            }
        }

        public void InitWeights(InitWeightsMode mode)
        {
            _outputLayer.InitWeights(mode);
            foreach (var h in _hiddenLayers) h.InitWeights(mode);
        }

        /// <summary>
        /// Получить выходы слоя
        /// </summary>
        /// <param name="layer">Слой: 0-выходной, 1...-скрытые, -1-входной</param>
        /// <returns>Массив чисел. Если массив размером 0 - неверный аргумент</returns>
        public double[] GetOutputs(int layer = 0)
        {
            if (layer == 0)
            {
                return _outputLayer.Outputs;
            }
            
            if (layer == -1)
            {
                return _inputLayer.Outputs;
            }

            if (layer - 1 < _hiddenLayers.Count)
            {
                return _hiddenLayers[layer - 1].Outputs;
            }

            return new double[0];
        }

        public void Train(double[] inputs, double[] targets)
        {
            Exec(inputs);

            _outputLayer.Train(targets);
            for(int i = _hiddenLayers.Count - 1; i >= 0; --i)
            {
                _hiddenLayers[i].Train();
            }
        }

        public double[] Execute(double[] inputs)
        {
            Exec(inputs);
            return _outputLayer.Outputs;
        }

        private void Exec(double[] inputs)
        {
            _inputLayer.Inputs = inputs;
            for (int i = 0; i < _hiddenLayers.Count; ++i)
            {
                _hiddenLayers[i].Execute();
            }
            _outputLayer.Execute();
        }
    }
}
