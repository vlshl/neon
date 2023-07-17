using Common;
using System;

namespace Core
{
    public class Trainer
    {
        private readonly IDataset _dataset;
        private readonly INetwork _network;
        private readonly INetworkConverter _converter; 

        public Trainer(IDataset dataset, INetwork network, INetworkConverter converter)
        {
            _dataset = dataset;
            _network = network;
            _converter = converter;
        }

        public void TrainEpoch(CancellationToken cancel)
        {
            TrainEpoch(1, cancel);
        }

        public void TrainEpoch(int epochs, CancellationToken cancel)
        {
            for (int ep = 0; ep < epochs; ++ep) 
            {
                if (cancel.IsCancellationRequested) break;

                bool isFirst = _dataset.First();
                if (isFirst)
                {
                    do
                    {
                        var sample = _dataset.GetCurrentSample();
                        if (sample == null) continue;

                        double[] targets = _converter.ConvertBack(sample.Label);
                        var inputs = sample.GetData();
                        _network.Train(inputs, targets);
                    } while (_dataset.Next());
                }
            }
        }
    }
}