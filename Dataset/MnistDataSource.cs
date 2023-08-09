using Common;

namespace Dataset
{
    public class MnistDataSource : IDataSource
    {
        private readonly IDataset _dataset;
        private int[] _indexes;
        private int _currentIndex;

        public MnistDataSource(IDataset dataset, int[] indexes)
        {
            _dataset = dataset;
            SetIndexes(indexes);
        }

        public MnistDataSource(IDataset dataset) : this(dataset, new int[0])
        {
        }

        public void SetIndexes(int[] indexes)
        {
            _indexes = indexes;
            _currentIndex = indexes.Any() ? 0 : -1;
        }

        public bool First()
        {
            if (_indexes.Any())
            {
                _currentIndex = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Last()
        {
            if (_indexes.Any())
            {
                _currentIndex = _indexes.Count() - 1;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Next()
        {
            if (_indexes.Any() && (_currentIndex < _indexes.Count() - 1))
            {
                _currentIndex++;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Prev()
        {
            if (_indexes.Any() && (_currentIndex > 0))
            {
                _currentIndex--;
                return true;
            }
            else
            {
                return false;
            }
        }


        public int GetCount()
        {
            return _indexes.Count();
        }

        public Sample? GetCurrentSample()
        {
            if (_currentIndex < 0) return null;

            return _dataset.GetSample(_indexes[_currentIndex]);
        }
    }
}
