using Common;
using Core;
using DynamicData;
using DynamicData.Binding;
using NeonUI.Views;
using Perz;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public ICommand TestCommand { get; set; }
    public ICommand SaveAsNetworkCommand { get; set; }
    public ICommand RefreshDatasetItemsCommand { get; set; }

    private INetwork? _nn;
    private string _layerSizes;
    private ComboBoxItem? _selectedDatasetItem;
    private ComboBoxItem? _selectedTrainDatasetItem;
    private ComboBoxItem? _selectedTestDatasetItem;
    private string _selectedDataKey;
    private DatasetManager _dsMan;
    private NetworkManager _nnMan;
    private string[] _dataKeyList;
    private string _execCurRes;
    private string _testRes;
    private string _saveAsNetworkName;

    public PerzNeuronetVM()
    {
        _nn = null;
        _dsMan = DatasetManager.Instance;
        _nnMan = NetworkManager.Instance;
        _layerSizes = "";
        _selectedDatasetItem = null;;
        _selectedTrainDatasetItem = null;
        _selectedTestDatasetItem = null;
        _selectedDataKey = "";
        _dataKeyList = new string[0];
        _execCurRes = "";
        _testRes = "";

        DatasetItems = new ObservableCollection<ComboBoxItem>();
        TrainDatasetItems = new ObservableCollection<ComboBoxItem>();
        TestDatasetItems = new ObservableCollection<ComboBoxItem>();
        RefreshDatasetItems();

        ExecuteCurrentCommand = ReactiveCommand.Create(ExecuteCurrent);
        ViewDataCommand = ReactiveCommand.Create(ViewData);
        TrainEpochCommand = ReactiveCommand.Create(TrainEpoch);
        SaveAsNetworkCommand = ReactiveCommand.Create(SaveAsNetwork);
        RefreshDatasetItemsCommand = ReactiveCommand.Create(RefreshDatasetItems);
        TestCommand = ReactiveCommand.Create(Test);
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
    public ObservableCollection<ComboBoxItem> DatasetItems { get; private set; }
    public ComboBoxItem? SelectedDatasetItem { get => _selectedDatasetItem; set => this.RaiseAndSetIfChanged(ref _selectedDatasetItem, value); }
    public ObservableCollection<ComboBoxItem> TrainDatasetItems { get; private set; }
    public ComboBoxItem? SelectedTrainDatasetItem { get => _selectedTrainDatasetItem; set => this.RaiseAndSetIfChanged(ref _selectedTrainDatasetItem, value); }
    public ObservableCollection<ComboBoxItem> TestDatasetItems { get; private set; }
    public ComboBoxItem? SelectedTestDatasetItem { get => _selectedTestDatasetItem; set => this.RaiseAndSetIfChanged(ref _selectedTestDatasetItem, value); }
    public string ExecuteCurrentResult { get => _execCurRes; set => this.RaiseAndSetIfChanged(ref _execCurRes, value); }
    public string SelectedDataKey { get => _selectedDataKey; set => this.RaiseAndSetIfChanged(ref _selectedDataKey, value); }
    public string[] DataKeyItems { get => _dataKeyList; set => this.RaiseAndSetIfChanged(ref _dataKeyList, value); }
    public string SaveAsNetworkName { get => _saveAsNetworkName; set => this.RaiseAndSetIfChanged(ref _saveAsNetworkName, value); }
    public string TestResult { get => _testRes; set => this.RaiseAndSetIfChanged(ref _testRes, value); }

    private void ExecuteCurrent()
    {
        if (_nn == null) return;
        if (SelectedDatasetItem == null) return;

        var ds = _dsMan.GetDataset(SelectedDatasetItem.Key);
        if (ds == null) return;

        var sample = ds.GetCurrentSample();
        if (sample == null) return;

        var res = _nn.Execute(sample.GetData());
        _nnMan.OnExec(_nn);

        PerzConverter conv = new PerzConverter(ds.GetAllLabels());
        var resLabel = conv.Convert(res);
        var targetLabel = sample.Label;

        ExecuteCurrentResult = "Ответ сети: " + resLabel + ", правильный ответ: " + targetLabel + ", " + (resLabel == targetLabel ? "Совпадение" : "Ошибка");
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
        if (_nn == null) return;
        if (_selectedTrainDatasetItem == null) return;

        var ds = _dsMan.GetDataset(_selectedTrainDatasetItem.Key);
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

    private void Test()
    {
        if (_nn == null) return;
        if (_selectedTestDatasetItem == null) return;

        var ds = _dsMan.GetDataset(_selectedTestDatasetItem.Key);
        if (ds == null) return;

        var conv = new PerzConverter(ds.GetAllLabels());
        CancellationTokenSource src = new CancellationTokenSource();
        var results = new Tester(ds, _nn, conv).Test(src.Token);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Метка\tУспешно\tОшибок\tВсего");
        foreach(var label in results.Keys)
        {
            var r = results[label];
            int total = r.Success + r.Error;
            sb.Append(label + "\t");
            sb.Append(string.Format("{0} ({1:F2} %)\t", r.Success, (decimal)r.Success / total * 100));
            sb.Append(string.Format("{0} ({1:F2} %)\t", r.Error, (decimal)r.Error / total * 100));
            sb.Append(total);
            sb.AppendLine();
        }
        
        TestResult = sb.ToString();
    }

    private void RefreshDatasetItems()
    {
        var list = _dsMan.GetDatasetPaths().Select(p => new ComboBoxItem(p, _dsMan.GetDatasetName(p)));
        DatasetItems.Clear();
        DatasetItems.AddRange(list);

        TrainDatasetItems.Clear();
        TrainDatasetItems.AddRange(list);

        TestDatasetItems.Clear();
        TestDatasetItems.AddRange(list);
    }
}
