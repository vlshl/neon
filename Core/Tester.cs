using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class Tester
    {
        public Action<long, long>? OnProgress;
        
        private readonly IDataset _dataset;
        private readonly INetwork _network;
        private readonly INetworkConverter _converter;
        private const long ONPROGRESS_STEP = TimeSpan.TicksPerSecond;

        public Tester(IDataset dataset, INetwork network, INetworkConverter converter)
        {
            _dataset = dataset;
            _network = network;
            _converter = converter;
            OnProgress = null;
        }

        public Task<Dictionary<string, TestResult>?> Test(CancellationToken cancel)
        {
            return Task.Run(() =>
            {
                Dictionary<string, TestResult> results = new Dictionary<string, TestResult>();

                bool isFirst = _dataset.First();
                if (!isFirst) return results;

                long ts1 = DateTime.Now.Ticks + ONPROGRESS_STEP;

                long totalCount = _dataset.GetCount();
                long count = 0;

                do
                {
                    if (cancel.IsCancellationRequested) break;

                    var sample = _dataset.GetCurrentSample();
                    if (sample == null) continue;

                    var inputs = sample.GetData();
                    var label = sample.Label;
                    var outputs = _network.Execute(inputs);
                    var resLabel = _converter.Convert(outputs);

                    if (!results.ContainsKey(label))
                    {
                        results.Add(label, new TestResult());
                    }
                    var r = results[label];

                    if (label == resLabel)
                    {
                        r.Success++;
                    }
                    else
                    {
                        r.Error++;
                    }

                    count++;
                    long ts = DateTime.Now.Ticks;
                    if (ts > ts1)
                    {
                        ts1 = ts + ONPROGRESS_STEP;
                        OnProgress?.Invoke(count, totalCount);
                    }
                } while (_dataset.Next());

                OnProgress?.Invoke(count, totalCount);
                NetworkManager.Instance.OnExec(_network);

                return !cancel.IsCancellationRequested ? results : null;
            });
        }
    }

    public class TestResult
    {
        public int Error { get; set; }
        public int Success { get; set; }
    }
}
