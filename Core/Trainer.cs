using Common;
using System;

namespace Core
{
    public class Trainer
    {
        public Action<long, long>? OnProgress;

        private readonly IDataset _dataset;
        private readonly INetwork _network;
        private readonly INetworkConverter _converter;
        private const long ONPROGRESS_STEP = TimeSpan.TicksPerSecond;

        public Trainer(IDataset dataset, INetwork network, INetworkConverter converter)
        {
            _dataset = dataset;
            _network = network;
            _converter = converter;
            OnProgress = null;
        }

        public Task<bool> TrainEpoch(CancellationToken cancel)
        {
            return TrainEpoch(1, cancel);
        }

        public Task<bool> TrainEpoch(int epochs, CancellationToken cancel)
        {
            return Task.Run(() =>
            {
                long ts1 = DateTime.Now.Ticks + ONPROGRESS_STEP;

                long totalCount = _dataset.GetCount() * epochs;
                long count = 0;
                _dataset.SuspendEvents();

                for (int ep = 0; ep < epochs; ++ep)
                {
                    if (cancel.IsCancellationRequested) break;

                    bool isFirst = _dataset.First();
                    if (isFirst)
                    {
                        do
                        {
                            if (cancel.IsCancellationRequested) break;

                            var sample = _dataset.GetCurrentSample();
                            if (sample == null) continue;

                            double[] targets = _converter.ConvertBack(sample.Label);
                            var inputs = sample.GetData();
                            _network.Train(inputs, targets);
                            count++;

                            long ts = DateTime.Now.Ticks;
                            if (ts > ts1)
                            {
                                ts1 = ts + ONPROGRESS_STEP;
                                OnProgress?.Invoke(count, totalCount);
                            }
                        } while (_dataset.Next());
                    }
                }

                _dataset.ResumeEvents();
                OnProgress?.Invoke(count, totalCount);
                NetworkManager.Instance.OnTrain(_network);

                return !cancel.IsCancellationRequested;
            });
        }
    }
}
