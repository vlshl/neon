namespace Perz
{
    public class PerzSettings
    {
        public string Name { get; set; }
        public int InputSize { get; set; }
        public int OutputSize { get; set; }
        public List<int> HiddenLayerSizes { get; set; }

        public PerzSettings()
        {
            Name = string.Empty;
            InputSize = 1;
            OutputSize = 1;
            HiddenLayerSizes = new List<int>();
        }
    }
}
