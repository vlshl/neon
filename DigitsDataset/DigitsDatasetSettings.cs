using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitsDs
{
    public class DigitsDatasetSettings
    {
        public string Dir { get; set; }
        public string ImagesFilename { get; set; }
        public string LabelsFilename { get; set; }

        public DigitsDatasetSettings()
        {
            Dir = string.Empty;
            ImagesFilename = string.Empty;
            LabelsFilename = string.Empty;
        }
    }
}
