using Core;
using Perz;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UI.ViewModels
{
    public class NewPerzNeuronetVM : DialogViewModel
    {
        public string Name { get; set; }
        public int InputLayerSize { get; set; }
        public int OutputLayerSize { get; set; }
        public string HiddenLayers { get; set; }
        public string InputLayerXY { get; set; }
        public string HiddenLayersXY { get; set; }
        public string OutputLayerXY { get; set; }

        private List<int> _hiddenLayerSizes;

        public NewPerzNeuronetVM()
        {
            Name = string.Empty;
            InputLayerSize = 1;
            InputLayerXY = "1x1";
            OutputLayerSize = 1;
            OutputLayerXY = "1x1";
            HiddenLayers = string.Empty;
            HiddenLayersXY = string.Empty;
            _hiddenLayerSizes = new List<int>();
        }

        public override void OnOk()
        {
            if (string.IsNullOrEmpty(Name))
            {
                MessagePanel?.ShowMessage("Не введено наименование");
                return;
            }

            if (InputLayerSize <= 0)
            {
                MessagePanel?.ShowMessage("Неверно указан размер входного слоя");
                return;
            }

            int[] inputXY = new int[0];
            if (!string.IsNullOrEmpty(InputLayerXY))
            {
                var xy = ParseXY(InputLayerXY);
                if (xy.Length != 2)
                {
                    MessagePanel?.ShowMessage("Неверно указан размер XY входного слоя");
                    return;
                }
                inputXY = xy;
            }

            if (OutputLayerSize <= 0)
            {
                MessagePanel?.ShowMessage("Неверно указан размер выходного слоя");
                return;
            }

            int[] outputXY = new int[0];
            if (!string.IsNullOrEmpty(OutputLayerXY))
            {
                var xy = ParseXY(OutputLayerXY);
                if (xy.Length != 2)
                {
                    MessagePanel?.ShowMessage("Неверно указан размер XY выходного слоя");
                    return;
                }
                outputXY = xy;
            }

            if (HiddenLayers.Trim() == string.Empty)
            {
                _hiddenLayerSizes.Clear();
            }
            else
            {
                int n;
                string[] parts = Regex.Split(HiddenLayers, @"\s*,\s*");
                foreach (string p in parts)
                {
                    if (!int.TryParse(p, out n))
                    {
                        MessagePanel?.ShowMessage("Неверно указан размер скрытого слоя");
                        continue;
                    }

                    _hiddenLayerSizes.Add(n);
                }
            }

            List<int[]> hiddensXY = new List<int[]>();
            if (!string.IsNullOrEmpty(HiddenLayersXY))
            {
                var xys = ParseXYs(HiddenLayersXY);
                if (xys.Count != _hiddenLayerSizes.Count)
                {
                    MessagePanel?.ShowMessage("Неверно указаны размеры XY скрытых слев");
                    return;
                }
                hiddensXY = xys;
            }

            PerzSettings settings = new PerzSettings();
            settings.InputSize = InputLayerSize;
            settings.InputSizeXY = inputXY;
            settings.OutputSize = OutputLayerSize;
            settings.OutputSizeXY = outputXY;
            foreach (var n in _hiddenLayerSizes)
            {
                settings.HiddenLayerSizes.Add(n);
            }
            settings.HiddenLayerSizesXY = hiddensXY;

            bool isSuccess = NetworkManager.Instance.CreateNetwork(Name.Trim(), settings);

            if (isSuccess)
            {
                base.OnOk();
            }
            else
            {
                MessagePanel?.ShowMessage("Ошибка при создании сети");
            }
        }

        private int[] ParseXY(string xyStr)
        {
            List<int> list = new List<int>();
            string[] parts = Regex.Split(xyStr, @"\s*x\s*");
            int x;
            foreach (string p in parts)
            {
                if (int.TryParse(p, out x)) list.Add(x);
            }

            return list.ToArray();
        }

        private List<int[]> ParseXYs(string xysStr)
        {
            List<int[]> list = new List<int[]>();
            string[] parts = Regex.Split(xysStr, @"\s*,\s*");
            foreach (string p in parts)
            {
                var xy = ParseXY(p);
                if (xy.Length != 2) continue;
                list.Add(xy);
            }

            return list;
        }
    }
}
