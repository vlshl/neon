using Avalonia;
using Avalonia.Media.Imaging;
using Common;
using Core;
using NeonUI.Views;
using ReactiveUI;
using System.Linq;
using System.Windows.Input;

namespace NeonUI.ViewModels;

public class MnistDatasetVM : WindowViewModel
{
    private IDataset? _ds;
    private int _dsCount;
    private int _dsIndex;
    private string _info;
    private string _filter;
    private string _label;
    private string _title;
    private int _imageWidth;
    private int _imageHeight;
    private WriteableBitmap? _imageSource;

    public ICommand NextCommand { get; set; }
    public ICommand PrevCommand { get; set; }
    public ICommand FirstCommand { get; set; }
    public ICommand LastCommand { get; set; }
    public ICommand SetFilterCommand { get; set; }
    public ICommand ClearFilterCommand { get; set; }
    public ICommand CloseWinCommand { get; set; }
    public ICommand CloseDsCommand { get; set; }

    public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title, value); }
    public string Info { get => _info; set => this.RaiseAndSetIfChanged(ref _info, value); }
    public string Filter { get => _filter; set => this.RaiseAndSetIfChanged(ref _filter, value); }
    public string Label { get => _label; set => this.RaiseAndSetIfChanged(ref _label, value); }
    public int ImageWidth { get => _imageWidth; set => this.RaiseAndSetIfChanged(ref _imageWidth, value); }
    public int ImageHeight { get => _imageHeight; set => this.RaiseAndSetIfChanged(ref _imageHeight, value); }
    public WriteableBitmap? ImageSource { get => _imageSource; set => this.RaiseAndSetIfChanged(ref _imageSource, value); }

    public MnistDatasetVM()
    {
        _ds = null;
        _info = "";
        _filter = "";
        _label = "";
        _imageWidth = 0;
        _imageHeight = 0;
        _imageSource = null;
        _title = "";

        NextCommand = ReactiveCommand.Create(() => { _ds?.Next(); });
        PrevCommand = ReactiveCommand.Create(() => { _ds?.Prev(); });
        FirstCommand = ReactiveCommand.Create(() => { _ds?.First(); });
        LastCommand = ReactiveCommand.Create(() => { _ds?.Last(); });
        SetFilterCommand = ReactiveCommand.Create(SetFilter);
        ClearFilterCommand = ReactiveCommand.Create(ClearFilter);
        CloseWinCommand = ReactiveCommand.Create(() => { CloseWindow?.Invoke(); });
        CloseDsCommand = ReactiveCommand.Create(CloseDs);
    }

    public void Initialize(IDataset ds)
    {
        _ds = ds;
        _ds.OnFilterChange += _ds_OnFilterChange;
        _ds.OnCurrentChange += _ds_OnCurrentChange;
        Title = ds.GetName();
        _ds_OnFilterChange();
    }

    public void Close()
    {
        if (_ds == null) return;

        _ds.OnFilterChange -= _ds_OnFilterChange;
        _ds.OnCurrentChange -= _ds_OnCurrentChange;
    }

    private void CloseDs()
    {
        if (_ds == null) return;

        DatasetManager.Instance.CloseDataset(_ds);
        var mainVm = MainWindow.Instance.DataContext as MainVM;
        if (mainVm != null) mainVm.RefreshDsItems();
        CloseWindow?.Invoke();
    }

    private void _ds_OnCurrentChange()
    {
        ShowSample(_ds.GetCurrentSample());
    }

    private void _ds_OnFilterChange()
    {
        if (_ds == null) return;

        var f = _ds.GetFilter();
        if (f != null && f.Any())
        {
            Filter = f.First();
        }
        else
        {
            Filter = "";
        }

        RefreshInfo();
        ShowSample(_ds.GetCurrentSample());
    }

    private void RefreshInfo()
    {
        if (_ds == null) return;
        Info = string.Format("Элеметнов {0}, размер {1}x{2}", _ds.GetCount().ToString(), _ds.GetImageSizeX().ToString(), _ds.GetImageSizeY().ToString());
    }

    private void SetFilter()
    {
        _ds?.SetFilter(new string[] { _filter });
    }

    private void ClearFilter()
    {
        _ds?.ClearFilter();
        Filter = "";
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
