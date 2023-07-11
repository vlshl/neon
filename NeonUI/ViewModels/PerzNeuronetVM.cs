using Common;
using Perz;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeonUI.ViewModels;

public class PerzNeuronetVM : ViewModelBase
{
    private INetwork _nn;
    private string _layerSizes;

    public PerzNeuronetVM()
    {
        _layerSizes = "";
    }

    public void Initialize(INetwork nn)
    {
        _nn = nn;

        var settings = nn.GetSettings() as PerzSettings;
        if (settings == null) return;

        List<string> list = settings.HiddenLayerSizes.Select(r => r.ToString()).ToList();
        list.Insert(0, settings.InputSize.ToString());
        list.Add(settings.OutputSize.ToString());
        LayerSizes = string.Join("-", list);
    }

    public string LayerSizes { get => _layerSizes; set => this.RaiseAndSetIfChanged(ref _layerSizes, value); }


}

