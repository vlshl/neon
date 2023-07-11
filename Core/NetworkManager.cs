using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Perz;
using System.Data;

namespace Core
{
    public class NetworkManager
    {
        private static NetworkManager? _instance = null;
        private Dictionary<string, INetwork> _nets;
        private string _nnPath;

        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null) _instance = new NetworkManager();
                return _instance;
            }
        }

        public NetworkManager()
        {
            _nets = new Dictionary<string, INetwork>();
            _nnPath = string.Empty;
        }

        public void Initialize(string path)
        {
            _nnPath = path;
        }

        public bool CreateNetwork(string name, object settings)
        {
            if (settings is PerzSettings)
            {
                PerzSettings s = (PerzSettings)settings;
                var n = new PerzNetwork(s);
                n.LoadWeights(null);

                var nns = new NeuronetSettings();
                nns.Class = typeof(PerzNetwork).Name;
                nns.Settings = s;
                nns.Weights = n.GetWeights();

                string json = JsonConvert.SerializeObject(nns, Formatting.Indented);
                string filepath = Path.Combine(GetNeuronetDirectory(), name + "." + GetNeuronetFileExt());
                File.WriteAllText(filepath, json);

                return true;
            }

            return false;
        }

        public string? OpenNetwork(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (_nets.ContainsKey(name)) return null;

            string json = File.ReadAllText(path);
            var nns = JsonConvert.DeserializeObject<NeuronetSettings>(json);
            if (nns == null) return null;

            if (nns.Class == typeof(PerzNetwork).Name)
            {
                var settings = (nns.Settings as JObject).ToObject<PerzSettings>();
                if (settings == null) return null;

                PerzNetwork n = new PerzNetwork(settings);
                n.LoadWeights(nns.Weights);
                _nets[name] = n;

                return name;
            }

            return null;
        }

        //public void SaveNetwork(INetwork network, string path)
        //{
        //    var nns = new NeuronetSettings();
        //    nns.Class = network.GetType().Name;
        //    nns.Settings = network.GetSettings();
        //    nns.Weights = network.GetWeights();

        //    string json = JsonConvert.SerializeObject(nns, Formatting.Indented);
        //    File.WriteAllText(path, json);
        //}

        public void CloseNetwork(string name)
        {
            if (!_nets.ContainsKey(name)) { return; }
            _nets.Remove(name);
        }

        public INetwork? GetNetwork(string name)
        {
            if (!_nets.ContainsKey(name)) { return null; }
            return _nets[name];
        }

        public string GetNeuronetDirectory()
        {
            return _nnPath;
        }

        public string GetNeuronetFileExt()
        {
            return "nn";
        }

        public string[] GetNetworks()
        {
            return _nets.Keys.ToArray();
        }
    }

    public class NeuronetSettings
    {
        public string Class { get; set; }
        public object Settings { get; set; }
        public IEnumerable<double[,]> Weights { get; set; }
    }

}
