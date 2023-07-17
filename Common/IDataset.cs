namespace Common
{
    public delegate void DatasetChangeEH();

    public interface IDataset
    {
        void SetFilter(string[] labels);
        string[]? GetFilter();
        void ClearFilter();
        string[] GetAllLabels();
        int GetCount();
        IEnumerable<Sample> GetSamples(int skip = 0, int take = 0);
        Sample? GetCurrentSample();
        bool Next();
        bool Prev();
        bool First();
        bool Last();
        int GetImageSizeX();
        int GetImageSizeY();
        string GetName();
        object GetSettings();
        void Load();
        event DatasetChangeEH OnFilterChange;
        event DatasetChangeEH OnCurrentChange;
    }
}
