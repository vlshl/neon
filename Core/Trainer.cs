using Common;
using System;

namespace Core
{
    public class Trainer
    {
        public Action<long, long>? OnProgress;

        private readonly IDataset _dataset;
        private readonly INeuronet _network;
        private readonly INeuronetConverter _converter;
        private const long ONPROGRESS_STEP = TimeSpan.TicksPerSecond;

        public Trainer(IDataset dataset, INeuronet network, INeuronetConverter converter)
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
                var ds = _dataset.GetDataSource();
                long ts1 = DateTime.Now.Ticks + ONPROGRESS_STEP;

                long totalCount = ds.GetCount() * epochs;
                long count = 0;

                for (int ep = 0; ep < epochs; ++ep)
                {
                    if (cancel.IsCancellationRequested) break;

                    bool isFirst = ds.First();
                    if (isFirst)
                    {
                        do
                        {
                            if (cancel.IsCancellationRequested) break;

                            var sample = ds.GetCurrentSample();
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
                        } while (ds.Next());
                    }
                }

                OnProgress?.Invoke(count, totalCount);
                NeuronetManager.Instance.OnTrain(_network);

                return !cancel.IsCancellationRequested;
            });
        }
    }
}
