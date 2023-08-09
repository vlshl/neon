using Common;
using Dataset;
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

        public bool CreateDataset(string filename, object settings)
        {
            if (settings is MnistSettings)
            {
                MnistSettings s = (MnistSettings)settings;

                var dss = new DatasetSettings();
                dss.Class = typeof(MnistDataset).Name;
                dss.Settings = s;

                string json = JsonConvert.SerializeObject(dss, Formatting.Indented);
                string filepath = Path.Combine(GetDatasetDirectory(), filename + "." + GetDsFileExt());
                File.WriteAllText(filepath, json);

                return true;
            }

            return false;
        }

        public bool IsDatasetOpened(string path)
        {
            string fullpath = Path.GetFullPath(path);
            return _datasets.ContainsKey(fullpath);
        }

        public bool OpenDataset(string path)
        {
            string fullpath = Path.GetFullPath(path);
            if (_datasets.ContainsKey(fullpath)) return false;

            try
            {
                string json = File.ReadAllText(fullpath);
                var dss = JsonConvert.DeserializeObject<DatasetSettings>(json);
                if (dss == null) return false;

                if (dss.Class == typeof(MnistDataset).Name)
                {
                    var settings = (dss.Settings as JObject).ToObject<MnistSettings>();
                    if (settings == null) return false;

                    var ds = new MnistDataset(settings);
                    ds.Load();
                    _datasets[fullpath] = ds;

                    return true;
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Ошибка при открытии набора данных", ex);
            }

            return false;
        }

        public void CloseDataset(IDataset ds)
        {
            if (ds == null) return;

            var keys = _datasets.Where(r => r.Value == ds).Select(r => r.Key).ToList();
            foreach (var key in keys)
            {
                _datasets.Remove(key);
            }
        }

        public IDataset? GetDataset(string path)
        {
            string fullpath = Path.GetFullPath(path);
            if (!_datasets.ContainsKey(fullpath)) { return null; }
            return _datasets[fullpath];
        }

        public string GetDatasetDirectory()
        {
            return _dsPath;
        }

        public string GetDsFileExt()
        {
            return "ds";
        }

        public string[] GetDatasetPaths()
        {
            return _datasets.Keys.ToArray();
        }

        public string GetDatasetName(string path)
        {
            string fullpath = Path.GetFullPath(path);
            if (!_datasets.ContainsKey(fullpath)) { return ""; }
            return _datasets[fullpath].GetName();
        }
    }

    public class DatasetSettings
    {
        public string Class { get; set; }
        public object Settings { get; set; }
    }

}
