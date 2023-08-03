namespace Dataset
{
    public class MnistSettings
    {
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public string ImagesFilename { get; set; }
        public string LabelsFilename { get; set; }

        public MnistSettings()
        {
            Name = string.Empty;
            FolderPath = string.Empty;
            ImagesFilename = string.Empty;
            LabelsFilename = string.Empty;
        }
    }
}
