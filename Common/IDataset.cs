namespace Common
{
    public delegate void DatasetChangeEH();

    public interface IDataset
    {
        void SetFilter(string[] labels);
        string[]? GetFilter();
        void ClearFilter();
        string[] GetAllLabels();
        Sample? GetSample(int index);
        IDataSource GetDataSource();
        IDataSource GetDefaultDataSource();
        int GetAllCount();
        int GetImageSizeX();
        int GetImageSizeY();
        string GetName();
        object GetSettings();
        void Load();
    }
}
