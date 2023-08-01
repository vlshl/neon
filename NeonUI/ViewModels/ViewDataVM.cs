using Avalonia;
using Avalonia.Media.Imaging;
using Common;
using Core;
using ReactiveUI;
using System;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Windows.Input;

namespace NeonUI.ViewModels
{
    public class ViewDataVM : ViewModelBase
    {
        private INetwork? _net;
        private string _dataKey;
        private int _imageWidth;
        private int _imageHeight;
        private int _width;
        private int _height;
        private WriteableBitmap? _imageSource;
        private string _title;

        public int ImageWidth { get => _imageWidth; set => this.RaiseAndSetIfChanged(ref _imageWidth, value); }
        public int ImageHeight { get => _imageHeight; set => this.RaiseAndSetIfChanged(ref _imageHeight, value); }
        public WriteableBitmap? ImageSource { get => _imageSource; set => this.RaiseAndSetIfChanged(ref _imageSource, value); }
        public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title, value); }
        public int Width { get => _width; set => this.RaiseAndSetIfChanged(ref _width, value); }
        public int Height { get => _height; set => this.RaiseAndSetIfChanged(ref _height, value); }

        public ViewDataVM()
        {
            _title = "";
            _dataKey = "";
            _width = _height = 100;
            Title = "";
        }

        public void Initialize(INetwork net, string dataKey)
        {
            _net = net;
            _dataKey = dataKey;
            Refresh();
            NetworkManager.Instance.OnNeuronetChange += OnNeuronetChange;
        }

        private void Refresh()
        {
            double[] arr = NetworkManager.Instance.GetDataByKey(_net, _dataKey);
            int arrSize;
            if (arr == null || !arr.Any()) return;

            arrSize = arr.Length;
            Title = _dataKey + " (" + arrSize.ToString() + ")";
            int s = (int)Math.Round(Math.Sqrt(arrSize));
            int sx, sy;
            if (s * s == arrSize)
            {
                sx = sy = s;
            }
            else
            {
                sx = arrSize; sy = 1;
            }

            Width = sx + 50;
            Height = sy + 50;
            ShowImage(arr, sx, sy);
        }

        private void OnNeuronetChange(INetwork net, string e)
        {
            if (net == _net) Refresh();
        }

        public void Close()
        {
            NetworkManager.Instance.OnNeuronetChange -= OnNeuronetChange;
        }

        private void ShowImage(double[] arr, int sizeX, int sizeY)
        {
            if (arr.Length == 0) return;

            double min = arr[0];
            double max = arr[0];

            for (int i = 1; i < arr.Length; ++i)
            {
                if (arr[i] < min) { min = arr[i]; }
                if (arr[i] > max) { max = arr[i]; }
            }

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
                            int idx = y * sizeX + x;
                            double d = 0;
                            if (idx < arr.Length)
                            {
                                d = (arr[idx] - min) / (max - min);
                            }
                            byte c = (byte)Math.Round(d * 255);
                            adr[y * fb.RowBytes + x * 4] = 0;
                            adr[y * fb.RowBytes + x * 4 + 1] = c;
                            adr[y * fb.RowBytes + x * 4 + 2] = 0;
                            adr[y * fb.RowBytes + x * 4 + 3] = 255;
                        }
                    }
                }
            }

            ImageSource = wb;
        }
    }
}
