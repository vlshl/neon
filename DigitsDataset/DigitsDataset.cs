using Common;
using Newtonsoft.Json;

namespace DigitsDs
{
    public class DigitsDataset : IDataset
    {
        private string[] _labels;
        private byte[][] _images;
        private int _imgSizeY, _imgSizeX;
        private string _name;
        private readonly DigitsDatasetSettings _settings;

        public DigitsDataset(string name, DigitsDatasetSettings settings)
        {
            _name = name;
            _settings = settings;
            _labels = new string[0];
            _images = new byte[0][];
            _imgSizeY = 0;
            _imgSizeX = 0;
            _name = string.Empty;
        }

        public object GetSettings()
        {
            return _settings;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetLabel(int index)
        {
            if (index < 0 || index >= _images.Length) return "";
            return _labels[index];
        }

        public double[] GetSample(int index)
        {
            if (index < 0 || index >= _images.Length) return null;
            var bytes = _images[index];

            return bytes.Select(b => (double)b / 255.0).ToArray();
        }

        public int GetSampleSize()
        {
            return _imgSizeX * _imgSizeY;
        }

        public int[,] GetImage(int index)
        {
            if (index < 0 || index >= _images.Length) return null;
            var bytes = _images[index];

            int[,] img = new int[_imgSizeX, _imgSizeY];
            for (int x = 0; x < _imgSizeX; ++x)
            {
                for (int y = 0; y < _imgSizeY; ++y)
                {
                    img[x, y] = bytes[y * _imgSizeX + x];
                }
            }

            return img;
        }

        public int GetImageSizeX()
        {
            return _imgSizeX;
        }

        public int GetImageSizeY()
        {
            return _imgSizeY;
        }

        public int GetDatasetSize()
        {
            return _images.Length;
        }

        public void Load()
        {
            try
            {
                LoadLabels(Path.Combine(_settings.Dir, _settings.LabelsFilename));
                LoadImages(Path.Combine(_settings.Dir, _settings.ImagesFilename));
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

            for (int i = 0; i < count; ++i)
            {
                _labels[i] = bytes[index++].ToString();
            }
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