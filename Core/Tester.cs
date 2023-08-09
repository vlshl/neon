using Common;

namespace Core
{
    public class Tester
    {
        public Action<long, long>? OnProgress;
        
        private readonly IDataset _dataset;
        private readonly INeuronet _network;
        private readonly INeuronetConverter _converter;
        private const long ONPROGRESS_STEP = TimeSpan.TicksPerSecond;

        public Tester(IDataset dataset, INeuronet network, INeuronetConverter converter)
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
                var ds = _dataset.GetDataSource();

                Dictionary<string, TestResult> results = new Dictionary<string, TestResult>();
                long ts1 = DateTime.Now.Ticks + ONPROGRESS_STEP;

                long totalCount = ds.GetCount();
                long count = 0;

                bool isFirst = ds.First();
                if (isFirst)
                {
                    do
                    {
                        if (cancel.IsCancellationRequested) break;

                        var sample = ds.GetCurrentSample();
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
                    } while (ds.Next());
                }

                OnProgress?.Invoke(count, totalCount);
                NeuronetManager.Instance.OnExec(_network);

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
