using Common;

namespace Dataset
{
    public class MnistDataset : IDataset
    {
        private string[] _labels;
        private byte[][] _images;
        private int _imgSizeX, _imgSizeY;
        private readonly MnistSettings _settings;
        private string[]? _filter;
        private List<int> _filterIndexes;
        private List<string> _allLabels;
        private MnistDataSource _defaultDataSource;

        public MnistDataset(MnistSettings settings)
        {
            _settings = settings;
            _labels = new string[0];
            _images = new byte[0][];
            _imgSizeX = 0;
            _imgSizeY = 0;

            _filter = null;
            _filterIndexes = new List<int>();
            _allLabels = new List<string>();
            _defaultDataSource = new MnistDataSource(this);
        }

        public void SetFilter(string[] filter)
        {
            if (filter == null || filter.Length == 0)
            {
                ClearFilter();
            }
            else
            {
                _filter = filter;
                _filterIndexes.Clear();
                for (int i = 0; i < _labels.Length; ++i)
                {
                    if (_filter.Contains(_labels[i]))
                    {
                        _filterIndexes.Add(i);
                    }
                }
                _defaultDataSource.SetIndexes(_filterIndexes.ToArray());
            }
        }

        public void ClearFilter()
        {
            _filter = null;
            _filterIndexes.Clear();
            for (int i = 0; i < _labels.Length; ++i)
            {
                _filterIndexes.Add(i);
            }
            _defaultDataSource.SetIndexes(_filterIndexes.ToArray());
        }

        public string[]? GetFilter()
        {
            return _filter;
        }

        public IDataSource GetDataSource()
        {
            return new MnistDataSource(this, _filterIndexes.ToArray());
        }

        public IDataSource GetDefaultDataSource()
        {
            return _defaultDataSource;
        }

        public string[] GetAllLabels()
        {
            return _allLabels.ToArray();
        }

        public int GetAllCount()
        {
            return _labels.Length;
        }

        public Sample? GetSample(int index)
        {
            if ((index < 0) || (index >= _labels.Length) || (index >= _images.Length)) return null;

            return new Sample(_labels[index], _images[index], _imgSizeX, _imgSizeY);
        }

        public string GetName()
        {
            return _settings.Name;
        }

        public object GetSettings()
        {
            return _settings;
        }

        public int GetImageSizeX()
        {
            return _imgSizeX;
        }

        public int GetImageSizeY()
        {
            return _imgSizeY;
        }

        public void Load()
        {
            try
            {
                LoadLabels(Path.Combine(_settings.FolderPath, _settings.LabelsFilename));
                LoadImages(Path.Combine(_settings.FolderPath, _settings.ImagesFilename));
                ClearFilter();
            }
            catch (Exception e)
            {
                throw new Exception("Dataset initialization error", e);
            }
        }

        private void LoadLabels(string filepath)
        {
            var bytes = File.ReadAllBytes(filepath);
            int index = 0;
            var msb = GetInt32(bytes, index);
            index += 4;
            var count = GetInt32(bytes, index);
            index += 4;
            _labels = new string[count];
            _allLabels.Clear();
            for (int i = 0; i < count; ++i)
            {
                string label = bytes[index++].ToString();
                if (!_allLabels.Contains(label))
                {
                    _allLabels.Add(label);
                }
                _labels[i] = label;
            }
            _allLabels.Sort();
        }

        private void LoadImages(string filepath)
        {
            var bytes = File.ReadAllBytes(filepath);
            int index = 0;
            var msb = GetInt32(bytes, index);
            index += 4;
            var count = GetInt32(bytes, index);
            index += 4;
            _imgSizeY = GetInt32(bytes, index);
            index += 4;
            _imgSizeX = GetInt32(bytes, index);
            index += 4;
            int imgSize = (int)(_imgSizeY * _imgSizeX);
            _images = new byte[count][];

            for (int i = 0; i < count; ++i)
            {
                _images[i] = bytes.Skip(index).Take(imgSize).ToArray();
                index += imgSize;
            }
        }

        private int GetInt32(byte[] array, int index)
        {
            byte b0 = array[index];
            byte b1 = array[index + 1];
            byte b2 = array[index + 2];
            byte b3 = array[index + 3];

            return b3 + b2 * 256 + b1 * 256 * 256 + b0 * 256 * 256 * 256;
        }
    }
}