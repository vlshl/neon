namespace Common
{
    public interface IDataset
    {
        string GetName();
        string GetLabel(int index);
        double[] GetSample(int index);
        int GetSampleSize();
        int[,] GetImage(int index);
        int GetImageSizeX();
        int GetImageSizeY();
        int GetDatasetSize();
        void Load();
        object GetSettings();
    }
}