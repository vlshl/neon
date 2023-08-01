using Common;
using Newtonsoft.Json;

namespace DigitsDs
{
    public class DigitsDataset : IDataset
    {
        private string[] _labels;
        private byte[][] _images;
        private int _imgSizeX, _imgSizeY;
        private string _name;
        private readonly DigitsDatasetSettings _settings;
        private string[]? _filter;
        private List<int> _filterIndexes;
        private int _currentIndex;
        private List<string> _allLabels;

        public event DatasetChangeEH? OnFilterChange;
        public event DatasetChangeEH? OnCurrentChange;

        public DigitsDataset(string name, DigitsDatasetSettings settings)
        {
            _name = name;
            _settings = settings;
            _labels = new string[0];
            _images = new byte[0][];
            _imgSizeX = 0;
            _imgSizeY = 0;

            _filter = null;
            _filterIndexes = new List<int>();
            _currentIndex = -1;
            _allLabels = new List<string>();

            OnFilterChange = null;
            OnCurrentChange = null;
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

                if (_filterIndexes.Any())
                {
                    _currentIndex = 0;
                }
                else
                {
                    _currentIndex = -1;
                }
                OnFilterChange?.Invoke();
                OnCurrentChange?.Invoke();
            }
        }

        public string[]? GetFilter()
        {
            return _filter;
        }

        public void ClearFilter()
        {
            _filter = null;
            _filterIndexes.Clear();
            if (_labels.Any())
            {
                _currentIndex = 0;
            }
            else
            {
                _currentIndex = -1;
            }
            OnFilterChange?.Invoke();
            OnCurrentChange?.Invoke();
        }

        public string[] GetAllLabels()
        {
            return _allLabels.ToArray();
        }

        public int GetCount()
        {
            if (_filter == null)
            {
                return _labels.Length;
            }
            else
            {
                return _filterIndexes.Count;
            }
        }

        public IEnumerable<Sample> GetSamples(int skip = 0, int take = 0)
        {
            List<Sample> samples = new List<Sample>();
            if (_filter == null)
            {
                for (int i = 0; i < _labels.Length; ++i)
                {
                    samples.Add(new Sample(_labels[i], _images[i], _imgSizeX, _imgSizeY));
                }
            }
            else
            {
                for (int k = 0; k < _filterIndexes.Count; ++k)
                {
                    int i = _filterIndexes[k];
                    samples.Add(new Sample(_labels[i], _images[i], _imgSizeX, _imgSizeY));
                }
            }

            if (take > 0)
            {
                return samples.Skip(skip).Take(take);
            }
            else
            {
                return samples.Skip(skip);
            }
        }

        public Sample? GetCurrentSample()
        {
            if (_currentIndex < 0) return null;

            if (_filter == null)
            {
                return new Sample(_labels[_currentIndex], _images[_currentIndex], _imgSizeX, _imgSizeY);
            }
            else
            {
                int idx = _filterIndexes[_currentIndex];
                return new Sample(_labels[idx], _images[idx], _imgSizeX, _imgSizeY);
            }
        }

        public bool Next()
        {
            if (_currentIndex < 0) return false;

            if (_filter == null)
            {
                if (_currentIndex < _labels.Length - 1)
                {
                    _currentIndex++;
                    OnCurrentChange?.Invoke();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (_currentIndex < _filterIndexes.Count - 1)
                {
                    _currentIndex++;
                    OnCurrentChange?.Invoke();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool Prev()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                OnCurrentChange?.Invoke();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool First()
        {
            if (_currentIndex >= 0)
            {
                _currentIndex = 0;
                OnCurrentChange?.Invoke();
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Last()
        {
            if (_currentIndex < 0) return false;

            if (_filter == null)
            {
                _currentIndex = _labels.Length - 1;
            }
            else
            {
                _currentIndex = _filterIndexes.Count - 1;
            }
            OnCurrentChange?.Invoke();

            return true;
        }

        public string GetName()
        {
            return _name;
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
                LoadLabels(Path.Combine(_settings.Dir, _settings.LabelsFilename));
                LoadImages(Path.Combine(_settings.Dir, _settings.ImagesFilename));
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