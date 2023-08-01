namespace Perz
{
    public class PerzSettings
    {
        public int InputSize { get; set; }
        public int OutputSize { get; set; }
        public List<int> HiddenLayerSizes { get; set; }

        public PerzSettings()
        {
            InputSize = 1;
            OutputSize = 1;
            HiddenLayerSizes = new List<int>();
        }
    }
}
