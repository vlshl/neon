﻿using Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Perz;
using System.Data;
using System.Net.NetworkInformation;

namespace Core
{
    public delegate void NeunonetChangeEH(INetwork net, string e);

    public class NetworkManager
    {
        private static NetworkManager? _instance = null;
        private Dictionary<string, INetwork> _nets;
        private string _nnPath;

        public event NeunonetChangeEH OnNeuronetChange;

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

        public void OnTrain(INetwork net)
        {
            OnNeuronetChange?.Invoke(net, "train");
        }

        public void OnExec(INetwork net)
        {
            OnNeuronetChange?.Invoke(net, "exec");
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
                nns.Weights = n.GetAllWeights();

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

        public bool SaveNetwork(INetwork net, string name)
        {
            if (net == null) return false;

            var nns = new NeuronetSettings();
            nns.Class = typeof(PerzNetwork).Name;
            nns.Settings = net.GetSettings();
            nns.Weights = net.GetAllWeights();

            string json = JsonConvert.SerializeObject(nns, Formatting.Indented);
            string filepath = Path.Combine(GetNeuronetDirectory(), name + "." + GetNeuronetFileExt());
            File.WriteAllText(filepath, json);

            return true;
        }

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



        private const string INPUT = "input";
        private const string OUTPUT = "output";
        private const string HIDDEN = "hidden";

        public string[] GetDataKeys(INetwork net)
        {
            List<string> keys = new List<string>();
            var pn = net as PerzNetwork;
            if (pn != null)
            {
                keys.Add(OUTPUT);
                keys.Add(INPUT);
                int hidCount = pn.GetHiddenLayersCount();
                for (int i = 0; i < hidCount; ++i)
                {
                    keys.Add(HIDDEN + (i + 1).ToString());
                }
            }

            return keys.ToArray();
        }

        public double[] GetDataByKey(INetwork net, string key)
        {
            var pn = net as PerzNetwork;
            if (pn != null)
            {
                if (key == INPUT) return pn.GetOutputs(-1);
                if (key == OUTPUT) return pn.GetOutputs(0);
                if (key.StartsWith(HIDDEN))
                {
                    string k = key.Remove(0, HIDDEN.Length);
                    int h;
                    if (int.TryParse(k, out h))
                    {
                        return pn.GetOutputs(h);
                    }
                }

            }

            return new double[0];
        }











    }

    public class NeuronetSettings
    {
        public string Class { get; set; }
        public object Settings { get; set; }
        public IEnumerable<double[,]> Weights { get; set; }
    }

}
