namespace Perz
{
    public class PerzSettings
    {
        public int InputSize { get; set; }
        public int[] InputSizeXY { get; set; }
        public int OutputSize { get; set; }
        public int[] OutputSizeXY { get; set; }
        public List<int> HiddenLayerSizes { get; set; }
        public List<int[]> HiddenLayerSizesXY { get; set; }

        public PerzSettings()
        {
            InputSize = 1;
            InputSizeXY = new int[] { 1, 1 }; 
            OutputSize = 1;
            OutputSizeXY = new int[] { 1, 1 };
            HiddenLayerSizes = new List<int>();
            HiddenLayerSizesXY = new List<int[]>();
        }
    }
}
