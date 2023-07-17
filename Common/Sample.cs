namespace Common
{
    public class Sample
    {
        public string Label { get; private set; }

        private byte[] _data;
        private int _imgSizeX, _imgSizeY;

        public Sample(string label, byte[] data, int imgSizeX, int imgSizeY)
        {
            if (string.IsNullOrEmpty(label)) throw new ArgumentException("label");
            if (data == null) throw new ArgumentNullException("data");
            if (data.Length != imgSizeX * imgSizeY) throw new ArgumentException("data.Length");

            Label = label;
            _data = data;
            _imgSizeX = imgSizeX;
            _imgSizeY = imgSizeY;
        }

        public int GetImageSizeX()
        {
            return _imgSizeX;
        }

        public int GetImageSizeY()
        {
            return _imgSizeY;
        }
        
        public byte[,] GetImage()
        {
            byte[,] img = new byte[_imgSizeX, _imgSizeY];
            for (int x = 0; x < _imgSizeX; ++x)
            {
                for (int y = 0; y < _imgSizeY; ++y)
                {
                    img[x, y] = _data[y * _imgSizeX + x];
                }
            }

            return img;
        }

        public double[] GetData()
        {
            return _data.Select(b => (double)b / 255.0).ToArray();
        }
    }
}
