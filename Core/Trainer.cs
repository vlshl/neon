using Common;

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
            int size = _dataset.GetDatasetSize();

            for (int ep = 0; ep < epochs; ++ep) 
            {
                if (cancel.IsCancellationRequested) break;

                for (int i = 0; i < size; ++i)
                {
                    if (cancel.IsCancellationRequested) break;
                    Train(i);
                }
            }
        }

        private void Train(int index)
        {
            var label = _dataset.GetLabel(index);
            double[] targets = _converter.ConvertBack(label);
            var inputs = _dataset.GetSample(index);
            _network.Train(inputs, targets);
        }

    }
}