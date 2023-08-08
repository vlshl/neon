using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Common;
using Core;
using DynamicData;
using NeonUI.Views;
using Perz;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;

namespace NeonUI.ViewModels;

public class PerzNeuronetVM : WindowViewModel
{
    public IMessagePanel? MessagePanel { get; set; }
    public ICommand ExecuteCurrentCommand { get; set; }
    public ICommand ViewDataCommand { get; set; }
    public ICommand TrainEpochCommand { get; set; }
    public ICommand TrainCancelCommand { get; set; }
    public ICommand InitWeightsCommand { get; set; }
    public ICommand TestCommand { get; set; }
    public ICommand TestCancelCommand { get; set; }
    public ICommand SaveAsCommand { get; set; }
    public ICommand CloseNetworkCommand { get; set; }
    public ICommand CloseWindowCommand { get; set; }
    public ICommand RefreshDatasetItemsCommand { get; set; }

    public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title, value); }

    private INeuronet? _nn;
    private string _layerSizes;
    private ComboBoxItem? _selectedDatasetItem;
    private string _selectedDataKey;
    private DatasetManager _dsMan;
    private NeuronetManager _nnMan;
    private string[] _dataKeyList;
    private string _execCurRes;
    private string _testRes;
    private bool _isZeroInitWeights;
    private decimal _trainPercent; // [0, 100]
    private decimal _testPercent; // [0, 100]
    private CancellationTokenSource? _trainCancelSrc;
    private CancellationTokenSource? _testCancelSrc;
    private string _title;

    public PerzNeuronetVM()
    {
        _nn = null;
        _dsMan = DatasetManager.Instance;
        _nnMan = NeuronetManager.Instance;
        _layerSizes = "";
        _selectedDatasetItem = null;;
        _selectedDataKey = "";
        _dataKeyList = new string[0];
        _execCurRes = "";
        _testRes = "";
        _isZeroInitWeights = true;
        _trainPercent = 0;
        _testPercent = 0;
        _trainCancelSrc = null;
        _testCancelSrc = null;
        _title = "";

        DatasetItems = new ObservableCollection<ComboBoxItem>();
        RefreshDatasetItems();

        ExecuteCurrentCommand = ReactiveCommand.Create(ExecuteCurrent);
        ViewDataCommand = ReactiveCommand.Create(ViewData);
        TrainEpochCommand = ReactiveCommand.Create(TrainEpoch);
        TrainCancelCommand = ReactiveCommand.Create(TrainCancel);
        InitWeightsCommand = ReactiveCommand.Create(InitWeights);
        SaveAsCommand = ReactiveCommand.Create(SaveAs);
        CloseNetworkCommand = ReactiveCommand.Create(CloseNetwork);
        CloseWindowCommand = ReactiveCommand.Create(() => { CloseWindow?.Invoke(); });
        RefreshDatasetItemsCommand = ReactiveCommand.Create(RefreshDatasetItems);
        TestCommand = ReactiveCommand.Create(Test);
        TestCancelCommand = ReactiveCommand.Create(TestCancel);
    }

    public void Initialize(INeuronet nn)
    {
        _nn = nn;
        Title = nn.GetName();

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
    public string ExecuteCurrentResult { get => _execCurRes; set => this.RaiseAndSetIfChanged(ref _execCurRes, value); }
    public string SelectedDataKey { get => _selectedDataKey; set => this.RaiseAndSetIfChanged(ref _selectedDataKey, value); }
    public string[] DataKeyItems { get => _dataKeyList; set => this.RaiseAndSetIfChanged(ref _dataKeyList, value); }
    public string TestResult { get => _testRes; set => this.RaiseAndSetIfChanged(ref _testRes, value); }
    public bool IsZeroInitWeights { get => _isZeroInitWeights; set => this.RaiseAndSetIfChanged(ref _isZeroInitWeights, value); }
    public decimal TrainPercent { get => _trainPercent; set => this.RaiseAndSetIfChanged(ref _trainPercent, value); }
    public decimal TestPercent { get => _testPercent; set => this.RaiseAndSetIfChanged(ref _testPercent, value); }

    private void ExecuteCurrent()
    {
        if (_nn == null) return;
        if (SelectedDatasetItem == null)
        {
            MessagePanel?.ShowMessage("Не выбран источник данных");
            return;
        }

        var ds = _dsMan.GetDataset(SelectedDatasetItem.Key);
        if (ds == null)
        {
            MessagePanel?.ShowMessage("Источник данных закрыт, обновите список.");
            return;
        }

        var sample = ds.GetCurrentSample();
        if (sample == null)
        {
            MessagePanel?.ShowMessage("Не найден текущий элемент в источник данных.");
            return;
        }

        var res = _nn.Execute(sample.GetData());
        _nnMan.OnExec(_nn);

        PerzConverter conv = new PerzConverter(ds.GetAllLabels());
        var resLabel = conv.Convert(res);
        var targetLabel = sample.Label;

        ExecuteCurrentResult = "Ответ сети: " + resLabel + ", правильный ответ: " + targetLabel + ", " + (resLabel == targetLabel ? "Совпадение" : "Ошибка");
    }

    private void ViewData()
    {
        if (_nn == null) return;
        if (SelectedDataKey == "") return;

        ViewDataWindow win = new ViewDataWindow();
        win.Initialize(_nn, SelectedDataKey);
        win.Show(MainWindow.Instance);
    }

    private async void TrainEpoch()
    {
        if (_nn == null) return;
        if (SelectedDatasetItem == null)
        {
            MessagePanel?.ShowMessage("Не выбран источник данных");
            return;
        }

        var ds = _dsMan.GetDataset(SelectedDatasetItem.Key);
        if (ds == null)
        {
            MessagePanel?.ShowMessage("Источник данных закрыт, обновите список.");
            return;
        }

        var conv = new PerzConverter(ds.GetAllLabels());
        _trainCancelSrc = new CancellationTokenSource();
        var trainer = new Trainer(ds, _nn, conv);
        trainer.OnProgress = OnTrainProgress;
        ds.SuspendEvents();
        bool isComplete = await trainer.TrainEpoch(_trainCancelSrc.Token);
        ds.ResumeEvents();
        MessagePanel?.ShowMessage(isComplete ? "Процесс тренировки завершен" : "Процесс тренировки прерван");
    }

    private void OnTrainProgress(long count, long total)
    {
        decimal p = 0;
        if (total > 0)
        {
            p = (decimal)count / total * 100;
        }

        Dispatcher.UIThread.Invoke(() => { TrainPercent = p; });
    }

    private void TrainCancel()
    {
        _trainCancelSrc?.Cancel();
    }

    private async void SaveAs()
    {
        if (_nn == null) return;

        var sp = MainWindow.Instance.StorageProvider;
        IStorageFolder? dir = await sp.TryGetFolderFromPathAsync(NeuronetManager.Instance.GetNeuronetDirectory());
        if (dir == null) return;

        var file = await sp.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Сеть",
            DefaultExtension = NeuronetManager.Instance.GetNeuronetFileExt(),
            ShowOverwritePrompt = true,
            SuggestedStartLocation = dir
        });
        if (file == null) return;

        try
        {
            _nnMan.SaveAsNeuronet(_nn, file.Path.AbsolutePath);
        }
        catch(Exception ex)
        {
            MessagePanel?.ShowException(ex);
        }
    }

    private void CloseNetwork()
    {
        if (_nn == null) return;
        _nnMan.CloseNeuronet(_nn);

        var mainVm = MainWindow.Instance.DataContext as MainVM;
        if (mainVm != null) mainVm.RefreshNnItems();
        CloseWindow?.Invoke();
    }

    private async void Test()
    {
        if (_nn == null) return;
        if (SelectedDatasetItem == null)
        {
            MessagePanel?.ShowMessage("Не выбран источник данных");
            return;
        }

        var ds = _dsMan.GetDataset(SelectedDatasetItem.Key);
        if (ds == null)
        {
            MessagePanel?.ShowMessage("Источник данных закрыт, обновите список.");
            return;
        }

        var conv = new PerzConverter(ds.GetAllLabels());
        _testCancelSrc = new CancellationTokenSource();
        var tester = new Tester(ds, _nn, conv);
        tester.OnProgress = OnTesterProgress;
        ds.SuspendEvents();
        var results = await tester.Test(_testCancelSrc.Token);
        ds.ResumeEvents();
        if (results != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Метка\tУспешно\tОшибок\tВсего");
            foreach (var label in results.Keys)
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
            MessagePanel?.ShowMessage("Тестирование завершено");
        }
        else
        {
            MessagePanel?.ShowMessage("Тестирование прервано");
        }
    }

    private void OnTesterProgress(long count, long total)
    {
        decimal p = 0;
        if (total > 0)
        {
            p = (decimal)count / total * 100;
        }

        Dispatcher.UIThread.Invoke(() => { TestPercent = p; });
    }

    private void TestCancel()
    {
        _testCancelSrc?.Cancel();
    }

    private void InitWeights()
    {
        if (_nn == null) return;

        _nn.InitWeights(_isZeroInitWeights ? InitWeightsMode.Zero : InitWeightsMode.Random);
        _nnMan.OnTrain(_nn);
    }

    private void RefreshDatasetItems()
    {
        var list = _dsMan.GetDatasetPaths().Select(p => new ComboBoxItem(p, _dsMan.GetDatasetName(p)));
        DatasetItems.Clear();
        DatasetItems.AddRange(list);
    }

    public void Close()
    {
        _trainCancelSrc?.Cancel();
        _testCancelSrc?.Cancel();
    }
}
