using Avalonia;
using Avalonia.Media.Imaging;
using Common;
using Core;
using ReactiveUI;
using System.Windows.Input;

namespace NeonUI.ViewModels;

public class MnistDatasetVM : WindowViewModel
{
    public ICommand NextCommand { get; set; }
    public ICommand PrevCommand { get; set; }
    public ICommand FirstCommand { get; set; }
    public ICommand LastCommand { get; set; }
    public ICommand SetFilterCommand { get; set; }
    public ICommand ClearFilterCommand { get; set; }
    public ICommand CloseCommand { get; set; }

    public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title, value); }
    public string Info { get => _info; set => this.RaiseAndSetIfChanged(ref _info, value); }
    public string Filter { get => _filter; set => this.RaiseAndSetIfChanged(ref _filter, value); }
    public string Label { get => _label; set => this.RaiseAndSetIfChanged(ref _label, value); }
    public int ImageWidth { get => _imageWidth; set => this.RaiseAndSetIfChanged(ref _imageWidth, value); }
    public int ImageHeight { get => _imageHeight; set => this.RaiseAndSetIfChanged(ref _imageHeight, value); }
    public WriteableBitmap? ImageSource { get => _imageSource; set => this.RaiseAndSetIfChanged(ref _imageSource, value); }

    private IDataset? _dataset;
    private IDataSource? _dataSource;
    private string _info;
    private string _filter;
    private string _label;
    private string _title;
    private int _imageWidth;
    private int _imageHeight;
    private WriteableBitmap? _imageSource;

    public MnistDatasetVM()
    {
        _dataset = null;
        _dataSource = null;
        _info = "";
        _filter = "";
        _label = "";
        _imageWidth = 0;
        _imageHeight = 0;
        _imageSource = null;
        _title = "";

        NextCommand = ReactiveCommand.Create(Next);
        PrevCommand = ReactiveCommand.Create(Prev);
        FirstCommand = ReactiveCommand.Create(First);
        LastCommand = ReactiveCommand.Create(Last);
        SetFilterCommand = ReactiveCommand.Create(SetFilter);
        ClearFilterCommand = ReactiveCommand.Create(ClearFilter);
        CloseCommand = ReactiveCommand.Create(Close);
    }

    public void Initialize(IDataset dataset)
    {
        _dataset = dataset;
        _dataSource = dataset.GetDefaultDataSource();
        Title = dataset.GetName();
        RefreshInfo();
        ShowSample(_dataSource.GetCurrentSample());
    }

    private void Close()
    {
        CloseWindowAction?.Invoke();
    }

    public void OnCloseWindow()
    {
        if (_dataset != null) DatasetManager.Instance.CloseDataset(_dataset);
    }

    private void RefreshInfo()
    {
        if ((_dataSource == null) || (_dataset == null)) return;
        Info = string.Format("Элеметнов {0}, размер {1}x{2}", _dataSource.GetCount().ToString(), _dataset.GetImageSizeX().ToString(), _dataset.GetImageSizeY().ToString());
    }

    private void SetFilter()
    {
        if (_dataset == null || _dataSource == null) return;

        _dataset.SetFilter(new string[] { _filter });
        RefreshInfo();
        ShowSample(_dataSource.GetCurrentSample());
    }

    private void ClearFilter()
    {
        if (_dataset == null || _dataSource == null) return;

        _dataset.ClearFilter();
        Filter = "";
        RefreshInfo();
        ShowSample(_dataSource.GetCurrentSample());
    }

    private void Next()
    {
        if (_dataSource == null) return;
        if (_dataSource.Next())
        {
            ShowSample(_dataSource.GetCurrentSample());
        }
    }

    private void Prev()
    {
        if (_dataSource == null) return;
        if (_dataSource.Prev())
        {
            ShowSample(_dataSource.GetCurrentSample());
        }
    }

    private void First()
    {
        if (_dataSource == null) return;
        if (_dataSource.First())
        {
            ShowSample(_dataSource.GetCurrentSample());
        }
    }

    private void Last()
    {
        if (_dataSource == null) return;
        if (_dataSource.Last())
        {
            ShowSample(_dataSource.GetCurrentSample());
        }
    }

    private void ShowSample(Sample? sample)
    {
        if (sample == null) return;

        Label = sample.Label;

        var im = sample.GetImage();
        int sizeX = sample.GetImageSizeX();
        int sizeY = sample.GetImageSizeY();

        ImageWidth = sizeX * 5;
        ImageHeight = sizeY * 5;

        WriteableBitmap wb = new WriteableBitmap(new PixelSize(sizeX, sizeY), new Vector(96, 96));

        using (var fb = wb.Lock())
        {
            unsafe
            {
                byte* adr = (byte*)fb.Address;
                for (int x = 0; x < sizeX; ++x)
                {
                    for (int y = 0; y < sizeY; ++y)
                    {
                        byte c = (byte)im[x, y];
                        adr[y * fb.RowBytes + x * 4] = c;
                        adr[y * fb.RowBytes + x * 4 + 1] = c;
                        adr[y * fb.RowBytes + x * 4 + 2] = c;
                        adr[y * fb.RowBytes + x * 4 + 3] = 255;
                    }
                }
            }
        }

        ImageSource = wb;
    }
}
