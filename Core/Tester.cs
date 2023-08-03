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
        private readonly IDataset _dataset;
        private readonly INetwork _network;
        private readonly INetworkConverter _converter;

        public Tester(IDataset dataset, INetwork network, INetworkConverter converter)
        {
            _dataset = dataset;
            _network = network;
            _converter = converter;
        }

        public Dictionary<string, TestResult> Test(CancellationToken cancel)
        {
            Dictionary<string, TestResult> results = new Dictionary<string, TestResult>();

            bool isFirst = _dataset.First();
            if (!isFirst) return results;

            do
            {
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
            } while (_dataset.Next());

            NetworkManager.Instance.OnExec(_network);

            return results;
        }
    }

    public class TestResult
    {
        public int Error { get; set; }
        public int Success { get; set; }
    }
}
