using Common;
using DigitsDs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core
{
    public class DatasetManager
    {
        private static DatasetManager? _instance = null;
        private Dictionary<string, IDataset> _datasets;
        private string _dsPath;

        public static DatasetManager Instance
        {
            get
            {
                if (_instance == null) _instance = new DatasetManager();
                return _instance;
            }
        }

        private DatasetManager()
        {
            _datasets = new Dictionary<string, IDataset>();
            _dsPath = string.Empty;
        }

        public void Initialize(string dsPath)
        {
            _dsPath = dsPath;
        }

        public bool CreateDataset(string name, object settings)
        {
            if ( settings is DigitsDatasetSettings)
            {
                DigitsDatasetSettings s = (DigitsDatasetSettings)settings;

                var dss = new DatasetSettings();
                dss.Class = typeof(DigitsDataset).Name;
                dss.Settings = s;

                string json = JsonConvert.SerializeObject(dss, Formatting.Indented);
                string filepath = Path.Combine(GetDatasetDirectory(), name + "." + GetDsFileExt());
                File.WriteAllText(filepath, json);

                return true;
            }

            return false;
        }

        public string? OpenDataset(string path)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (_datasets.ContainsKey(name)) return null;

            string json = File.ReadAllText(path);
            var dss = JsonConvert.DeserializeObject<DatasetSettings>(json);
            if (dss == null) return null;

            if (dss.Class == typeof(DigitsDataset).Name)
            {
                var settings = (dss.Settings as JObject).ToObject<DigitsDatasetSettings>();
                if (settings == null) return null;

                var ds = new DigitsDataset(name, settings);
                ds.Load();
                _datasets[name] = ds;

                return name;
            }

            return null;
        }

        public void CloseDataset(string name)
        {
            if (!_datasets.ContainsKey(name)) { return; }
            _datasets.Remove(name);
        }

        public IDataset? GetDataset(string name)
        {
            if (!_datasets.ContainsKey(name)) { return null; }
            return _datasets[name];
        }

        public string GetDatasetDirectory()
        {
            return _dsPath;
        }

        public string GetDsFileExt()
        {
            return "ds";
        }

        public string[] GetDatasets()
        {
            return _datasets.Keys.ToArray();
        }
    }

    public class DatasetSettings
    {
        public string Class { get; set; }
        public object Settings { get; set; }
    }

}
