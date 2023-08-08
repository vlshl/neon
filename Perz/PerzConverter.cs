using Common;

namespace Perz
{
    public class PerzConverter : INeuronetConverter
    {
        private string[] _labels;
        private int _size;

        public PerzConverter(string[] labels)
        {
            if ((labels == null) || (labels.Length < 1)) 
                throw new ArgumentOutOfRangeException("labels");
            _labels = labels;
            _size = labels.Length;
        }

        public string Convert(double[] outputs)
        {
            if ((outputs == null) || (outputs.Length == 0)) return "";

            double max = outputs[0];
            int idx = 0;
            for (int i = 1; i < _size; ++i)
            {
                double d = i < outputs.Length ? outputs[i] : 0;
                if (d > max)
                {
                    idx = i;
                    max = d;
                }
            }

            return _labels[idx];
        }

        public double[] ConvertBack(string label)
        {
            double[] arr = new double[_size];
            for (int i = 0; i < _size; ++i) arr[i] = 0;


            int found = Array.FindIndex(_labels, r => r == label);
            if (found >= 0)
            {
                arr[found] = 1;
            }

            return arr;
        }
    }
}
