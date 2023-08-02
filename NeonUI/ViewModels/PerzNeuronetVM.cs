using Common;
using Core;
using NeonUI.Views;
using Perz;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NeonUI.ViewModels;

public class PerzNeuronetVM : WindowViewModel
{
    public ICommand ExecuteCurrentCommand { get; set; }
    public ICommand ViewDataCommand { get; set; }
    public ICommand TrainEpochCommand { get; set; }
    public ICommand SaveAsNetworkCommand { get; set; }

    private INetwork? _nn;
    private string _layerSizes;
    private string _selectedDs;
    private string _selectedTrainDs;
    private string _selectedDataKey;
    private DatasetManager _dsMan;
    private NetworkManager _nnMan;
    private string[] _dsList;
    private string[] _trainDsList;
    private string[] _dataKeyList;
    private string _execCurRes;
    private string _saveAsNetworkName;

    public PerzNeuronetVM()
    {
        _nn = null;
        _dsMan = DatasetManager.Instance;
        _nnMan = NetworkManager.Instance;
        _layerSizes = "";
        _selectedDs = "";
        _selectedTrainDs = "";
        _selectedDataKey = "";
        _dataKeyList = new string[0];
        _dsList = _dsMan.GetDatasets();
        SelectedDataset = _dsList.Any() ? _dsList[0] : "";
        _trainDsList = _dsMan.GetDatasets();
        SelectedTrainDataset = _trainDsList.Any() ? _trainDsList[0] : "";
        _execCurRes = "";

        ExecuteCurrentCommand = ReactiveCommand.Create(ExecuteCurrent);
        ViewDataCommand = ReactiveCommand.Create(ViewData);
        TrainEpochCommand = ReactiveCommand.Create(TrainEpoch);
        SaveAsNetworkCommand = ReactiveCommand.Create(SaveAsNetwork);
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

        DataKeyItems = _nnMan.GetDataKeys(_nn);
        SelectedDataKey = _dataKeyList.Any() ? _dataKeyList[0] : "";
    }

    public string LayerSizes { get => _layerSizes; set => this.RaiseAndSetIfChanged(ref _layerSizes, value); }
    public string SelectedDataset { get => _selectedDs; set => this.RaiseAndSetIfChanged(ref _selectedDs, value); }
    public string[] DatasetItems { get => _dsList; }
    public string ExecuteCurrentResult { get => _execCurRes; set => this.RaiseAndSetIfChanged(ref _execCurRes, value); }

    public string SelectedTrainDataset { get => _selectedTrainDs; set => this.RaiseAndSetIfChanged(ref _selectedTrainDs, value); }
    public string[] TrainDatasetItems { get => _trainDsList; }
    public string SelectedDataKey { get => _selectedDataKey; set => this.RaiseAndSetIfChanged(ref _selectedDataKey, value); }
    public string[] DataKeyItems { get => _dataKeyList; set => this.RaiseAndSetIfChanged(ref _dataKeyList, value); }
    public string SaveAsNetworkName { get => _saveAsNetworkName; set => this.RaiseAndSetIfChanged(ref _saveAsNetworkName, value); }

    private void ExecuteCurrent()
    {
        if (_nn == null) return;
        if (string.IsNullOrEmpty(SelectedDataset)) return;

        var ds = _dsMan.GetDataset(SelectedDataset);
        if (ds == null) return;

        var sample = ds.GetCurrentSample();
        if (sample == null) return;

        var res = _nn.Execute(sample.GetData());
        _nnMan.OnExec(_nn);

        PerzConverter conv = new PerzConverter(ds.GetAllLabels());
        var resLabel = conv.Convert(res);
        var targetLabel = sample.Label;

        ExecuteCurrentResult = "Получен ответ: " + resLabel + ", правильный ответ: " + targetLabel + ", " + (resLabel == targetLabel ? "Совпадение" : "Ошибка");
    }

    private void ViewData()
    {
        if (SelectedDataKey == "") return;

        ViewDataWindow win = new ViewDataWindow();
        win.Initialize(_nn, SelectedDataKey);
        win.Show(MainWindow.Instance);
    }

    private void TrainEpoch()
    {
        if (string.IsNullOrEmpty(_selectedTrainDs)) return;

        var ds = _dsMan.GetDataset(_selectedTrainDs);
        if (ds == null) return;

        var conv = new PerzConverter(ds.GetAllLabels());
        CancellationTokenSource src = new CancellationTokenSource();
        new Trainer(ds, _nn, conv).TrainEpoch(src.Token);
    }

    private void SaveAsNetwork()
    {
        if (string.IsNullOrEmpty(SaveAsNetworkName)) return;

        _nnMan.SaveNetwork(_nn, SaveAsNetworkName);
    }
}
