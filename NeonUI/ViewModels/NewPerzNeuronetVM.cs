using Core;
using Perz;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NeonUI.ViewModels;

public class NewPerzNeuronetVM : DialogViewModel
{
    public string Name { get; set; }
    public int InputLayerSize { get; set; }
    public int OutputLayerSize { get; set; }
    public string HiddenLayers { get; set; }

    private List<int> _hiddenLayerSizes;

    public NewPerzNeuronetVM()
    {
        Name = string.Empty;
        InputLayerSize = 1;
        OutputLayerSize = 1;
        HiddenLayers = string.Empty;
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

        if (OutputLayerSize <= 0)
        {
            MessagePanel?.ShowMessage("Неверно указан размер выходного слоя");
            return;
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

        PerzSettings settings = new PerzSettings();
        settings.InputSize = InputLayerSize;
        settings.OutputSize = OutputLayerSize;
        foreach (var n in _hiddenLayerSizes)
        {
            settings.HiddenLayerSizes.Add(n);
        }

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
}
