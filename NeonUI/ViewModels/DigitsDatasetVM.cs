using Avalonia;
using Avalonia.Media.Imaging;
using Common;
using ReactiveUI;
using System.Windows.Input;

namespace NeonUI.ViewModels;

public class DigitsDatasetVM : ViewModelBase
{
    private IDataset _ds;
    private int _dsCount;
    private int _dsIndex;
    private string _info;
    private string _index;
    private string _label;
    private int _imageWidth;
    private int _imageHeight;
    private WriteableBitmap? _imageSource;

    public ICommand PlusCommand { get; set; }
    public ICommand MinusCommand { get; set; }

    public string Info { get => _info; set => this.RaiseAndSetIfChanged(ref _info, value); }
    public string Index { get => _index; set => this.RaiseAndSetIfChanged(ref _index, value); }
    public string Label { get => _label; set => this.RaiseAndSetIfChanged(ref _label, value); }
    public int ImageWidth { get => _imageWidth; set => this.RaiseAndSetIfChanged(ref _imageWidth, value); }
    public int ImageHeight { get => _imageHeight; set => this.RaiseAndSetIfChanged(ref _imageHeight, value); }
    public WriteableBitmap? ImageSource { get => _imageSource; set => this.RaiseAndSetIfChanged(ref _imageSource, value); }

    public DigitsDatasetVM() 
    {
        _dsCount = 0;
        _dsIndex = 0;
        _info = "";
        _index = "";
        _label = "";
        _imageWidth = 0;
        _imageHeight = 0;
        _imageSource = null;

        PlusCommand = ReactiveCommand.Create(() => Plus());
        MinusCommand = ReactiveCommand.Create(() => Minus());
    }

    public void Initialize(IDataset ds)
    {
        _ds = ds;
        _dsCount = ds.GetDatasetSize();
        //Title = "Набор: " + ds.GetName();
        Info = "Элеметнов: " + _dsCount.ToString();
        ShowSample();
    }

    public void Plus()
    {
        _dsIndex++;

        if (_dsIndex < 0) _dsIndex = _dsCount - 1;
        if (_dsIndex >= _dsCount) _dsIndex = 0;

        ShowSample();
    }

    public void Minus()
    {
        _dsIndex--;

        if (_dsIndex < 0) _dsIndex = _dsCount - 1;
        if (_dsIndex >= _dsCount) _dsIndex = 0;

        ShowSample();
    }

    private void ShowSample()
    {
        Index = "Index: " + _dsIndex.ToString();
        Label = "Label: " + _ds.GetLabel(_dsIndex);

        var im = _ds.GetImage(_dsIndex);
        int sizeX = _ds.GetImageSizeX();
        int sizeY = _ds.GetImageSizeY();

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
