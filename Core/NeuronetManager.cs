using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Perz;
using System.Data;

namespace Core
{
    public delegate void NeunonetChangeEH(INeuronet net, string e);

    public class NeuronetManager
    {
        private static NeuronetManager? _instance = null;
        private Dictionary<string, INeuronet> _nets;
        private string _nnPath;

        public event NeunonetChangeEH? OnNeuronetChange;

        public static NeuronetManager Instance
        {
            get
            {
                if (_instance == null) _instance = new NeuronetManager();
                return _instance;
            }
        }

        public NeuronetManager()
        {
            _nets = new Dictionary<string, INeuronet>();
            _nnPath = string.Empty;
        }

        public void Initialize(string path)
        {
            _nnPath = path;
        }

        public void OnTrain(INeuronet net)
        {
            OnNeuronetChange?.Invoke(net, "train");
        }

        public void OnExec(INeuronet net)
        {
            OnNeuronetChange?.Invoke(net, "exec");
        }

        public bool CreateNeuronet(string filename, object settings)
        {
            if (settings is PerzSettings)
            {
                PerzSettings s = (PerzSettings)settings;
                var n = new PerzNeuronet(s);
                n.InitWeights(InitWeightsMode.Random);

                var nns = new NeuronetSettings();
                nns.Class = typeof(PerzNeuronet).Name;
                nns.Settings = s;
                nns.Weights = n.GetAllWeights();

                string json = JsonConvert.SerializeObject(nns, Formatting.Indented);
                string filepath = Path.Combine(GetNeuronetDirectory(), filename + "." + GetNeuronetFileExt());
                File.WriteAllText(filepath, json);

                return true;
            }

            return false;
        }

        public bool OpenNeuronet(string path)
        {
            string fullpath = Path.GetFullPath(path);
            if (_nets.ContainsKey(fullpath)) return false;

            try
            {
                string json = File.ReadAllText(fullpath);
                var nns = JsonConvert.DeserializeObject<NeuronetSettings>(json);
                if (nns == null) return false;

                if (nns.Class == typeof(PerzNeuronet).Name)
                {
                    var settings = (nns.Settings as JObject).ToObject<PerzSettings>();
                    if (settings == null) return false;

                    PerzNeuronet n = new PerzNeuronet(settings);
                    n.LoadWeights(nns.Weights);
                    _nets[fullpath] = n;

                    return true;
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Ошибка при открытии нейросети", ex);
            }

            return false;
        }

        public void SaveAsNeuronet(INeuronet net, string filepath)
        {
            if (net == null) return;

            var nns = new NeuronetSettings();
            nns.Class = typeof(PerzNeuronet).Name;
            nns.Settings = net.GetSettings();
            nns.Weights = net.GetAllWeights();

            string json = JsonConvert.SerializeObject(nns, Formatting.Indented);

            try
            {
                File.WriteAllText(filepath, json);
            }
            catch(Exception ex)
            {
                throw new Exception("Ошибка при сохранении файла", ex);
            }
        }

        public void CloseNeuronet(INeuronet n)
        {
            if (n == null) return;

            var keys = _nets.Where(r => r.Value == n).Select(r => r.Key).ToList();
            foreach (var key in keys)
            {
                _nets.Remove(key);
            }
        }

        public INeuronet? GetNeuronet(string path)
        {
            string fullpath = Path.GetFullPath(path);
            if (!_nets.ContainsKey(fullpath)) { return null; }
            return _nets[fullpath];
        }

        public string GetNeuronetDirectory()
        {
            return _nnPath;
        }

        public string GetNeuronetFileExt()
        {
            return "nn";
        }

        public string[] GetNeuronetPaths()
        {
            return _nets.Keys.ToArray();
        }

        public string GetNeuronetName(string path)
        {
            string fullpath = Path.GetFullPath(path);
            if (!_nets.ContainsKey(fullpath)) { return ""; }
            return _nets[fullpath].GetName();
        }

        private const string INPUT = "input";
        private const string OUTPUT = "output";
        private const string HIDDEN = "hidden";
        private const string OUTPUT_WEIGHTS = "output_weights";
        private const string HIDDEN_WEIGHTS = "hidden_weights";

        public string[] GetDataKeys(INeuronet net)
        {
            List<string> keys = new List<string>();
            var pn = net as PerzNeuronet;
            if (pn != null)
            {
                keys.Add(INPUT);
                keys.Add(OUTPUT);
                int hidCount = pn.GetHiddenLayersCount();
                for (int i = 0; i < hidCount; ++i)
                {
                    keys.Add(HIDDEN + ":" + (i + 1).ToString());
                }
                keys.Add(OUTPUT_WEIGHTS);
                for (int i = 0; i < hidCount; ++i)
                {
                    keys.Add(HIDDEN_WEIGHTS + ":" + (i + 1).ToString());
                }
            }

            return keys.ToArray();
        }

        public double[,] GetDataByKey(INeuronet net, string key)
        {
            var pn = net as PerzNeuronet;
            if (pn != null)
            {
                if (key == INPUT) return GetArray2(pn.GetOutputs(-1));
                if (key == OUTPUT) return GetArray2(pn.GetOutputs(0));
                if (key.StartsWith(HIDDEN + ":"))
                {
                    string k = key.Remove(0, HIDDEN.Length + 1);
                    int h;
                    if (int.TryParse(k, out h))
                    {
                        return GetArray2(pn.GetOutputs(h));
                    }
                }
                if (key == OUTPUT_WEIGHTS) return pn.GetWeights(0);
                if (key.StartsWith(HIDDEN_WEIGHTS + ":"))
                {
                    string k = key.Remove(0, HIDDEN_WEIGHTS.Length + 1);
                    int h;
                    if (int.TryParse(k, out h))
                    {
                        return pn.GetWeights(h);
                    }
                }
            }

            return new double[0, 0];
        }

        private double[,] GetArray2(double[] arr)
        {
            var arr2 = new double[1, arr.Length];
            for (int i = 0; i < arr.Length; ++i) arr2[0, i] = arr[i];
            return arr2;
        }
    }

    public class NeuronetSettings
    {
        public string Class { get; set; }
        public object Settings { get; set; }
        public IEnumerable<double[,]> Weights { get; set; }
    }

}
