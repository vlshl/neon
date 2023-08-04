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
    public class ViewDataVM : WindowViewModel
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
            double[,] arr = NetworkManager.Instance.GetDataByKey(_net, _dataKey);
            if ((arr == null) || (arr.GetLength(0) == 0) || (arr.GetLength(1) == 0)) return;

            int arrSize1 = arr.GetLength(0);
            int arrSize2 = arr.GetLength(1);
            Title = _dataKey + " (" + arrSize1.ToString() + "x" + arrSize2.ToString() + ")";

            int s1 = (int)Math.Round(Math.Sqrt(arrSize1));
            int sx1, sy1;
            if (s1 * s1 == arrSize1)
            {
                sx1 = sy1 = s1;
            }
            else
            {
                sx1 = arrSize1; sy1 = 1;
            }

            int s2 = (int)Math.Round(Math.Sqrt(arrSize2));
            int sx2, sy2;
            if (s2 * s2 == arrSize2)
            {
                sx2 = sy2 = s2;
            }
            else
            {
                sx2 = arrSize2; sy2 = 1;
            }

            Width = sx1 * sx2 + 50;
            Height = sy1 * sy2 + 50;
            ShowImage(arr, sx1, sy1, sx2, sy2);
        }

        private void OnNeuronetChange(INetwork net, string e)
        {
            if (net == _net) Refresh();
        }

        public void Close()
        {
            NetworkManager.Instance.OnNeuronetChange -= OnNeuronetChange;
        }

        private void ShowImage1(double[,] arr, int sizeX1, int sizeY1, int sizeX2, int sizeY2)
        {
            ImageWidth = sizeX1 * sizeX2 * 5;
            ImageHeight = sizeY1 * sizeY2 * 5;

            int len1 = arr.GetLength(0);
            int len2 = arr.GetLength(1);

            WriteableBitmap wb = new WriteableBitmap(new PixelSize(sizeX1 * sizeX2, sizeY1 * sizeY2), new Vector(96, 96));

            using (var fb = wb.Lock())
            {
                unsafe
                {
                    byte* adr = (byte*)fb.Address;

                    for (int y1 = 0; y1 < sizeY1; ++y1)
                    {
                        for (int x1 = 0; x1 < sizeX1; ++x1)
                        {
                            int idx1 = y1 * sizeX1 + x1;

                            double min = arr[idx1, 0];
                            double max = arr[idx1, 0];

                            for (int i = 1; i < len2; ++i)
                            {
                                if (arr[idx1, i] < min) { min = arr[idx1, i]; }
                                if (arr[idx1, i] > max) { max = arr[idx1, i]; }
                            }

                            for (int y2 = 0; y2 < sizeY2; ++y2)
                            {
                                for (int x2 = 0; x2 < sizeX2; ++x2)
                                {
                                    int idx2 = y2 * sizeX2 + x2;
                                    double dn = 0, dp = 0;
                                    if ((idx1 >= len1) || (idx2 >= len2)) continue;

                                    // отрицательные значения показываем градациями одного цвета, положительные градациями другого, ноль черным
                                    double d = arr[idx1, idx2];

                                    if (min < 0 && max <= 0)
                                    {
                                        if (max != min)
                                        {
                                            dn = Math.Abs((d - min) / (max - min));
                                        }
                                        else
                                        {
                                            dn = 1;
                                        }
                                        dp = 0;
                                    }
                                    else if (min >= 0 && max > 0)
                                    {
                                        if (max != min)
                                        {
                                            dp = Math.Abs((d - min) / (max - min));
                                        }
                                        else
                                        {
                                            dp = 1;
                                        }
                                        dn = 0;
                                    }
                                    else
                                    {
                                        if (d < 0)
                                        {
                                            if (min != 0)
                                            {
                                                dn = Math.Abs((d - min) / (-min));
                                            }
                                            else
                                            {
                                                dn = 0;
                                            }
                                            dp = 0;
                                        }
                                        else
                                        {
                                            dn = 0;
                                            if (max != 0)
                                            {
                                                dp = Math.Abs(d / max);
                                            }
                                            else
                                            {
                                                dp = 0;
                                            }
                                        }
                                    }

                                    byte r, g, b;
                                    if (d < 0)
                                    {
                                        r = 0;
                                        g = (byte)Math.Round((1 - dn) * 255);
                                        b = (byte)Math.Round(dn * 255);
                                    }
                                    else
                                    {
                                        r = (byte)Math.Round(dp * 255);
                                        g = (byte)Math.Round((1 - dp) * 255);
                                        b = 0;
                                    }

                                    int offset = (y1 * sizeY2 + y2) * fb.RowBytes + (x1 * sizeX2 + x2) * 4;
                                    adr[offset] = b; // blue
                                    adr[offset + 1] = g; // green
                                    adr[offset + 2] = r; // red
                                    adr[offset + 3] = 255; // alpha
                                }
                            }

                        }
                    }
                }
            }

            ImageSource = wb;
        }

        private void ShowImage(double[,] arr, int sizeX1, int sizeY1, int sizeX2, int sizeY2)
        {
            int scale = 5;
            ImageWidth = sizeX1 * sizeX2 * scale;
            ImageHeight = sizeY1 * sizeY2 * scale;

            int len1 = arr.GetLength(0);
            int len2 = arr.GetLength(1);

            WriteableBitmap wb = new WriteableBitmap(new PixelSize(sizeX1 * sizeX2 * scale, sizeY1 * sizeY2 * scale), new Vector(96, 96));

            using (var fb = wb.Lock())
            {
                unsafe
                {
                    byte* adr = (byte*)fb.Address;

                    for (int y1 = 0; y1 < sizeY1; ++y1)
                    {
                        for (int x1 = 0; x1 < sizeX1; ++x1)
                        {
                            int idx1 = y1 * sizeX1 + x1;

                            for (int y2 = 0; y2 < sizeY2; ++y2)
                            {
                                for (int x2 = 0; x2 < sizeX2; ++x2)
                                {
                                    int idx2 = y2 * sizeX2 + x2;
                                    if ((idx1 >= len1) || (idx2 >= len2)) continue;

                                    double d = arr[idx1, idx2];
                                    double dp = Math.Abs(d);
                                    if (dp > 1) dp = 1;

                                    var a = dp * Math.PI / 2;

                                    byte r, g, b;

                                    if (d < 0)
                                    {
                                        r = 0;
                                        g = (byte)Math.Round(Math.Cos(a) * 255);
                                        b = (byte)Math.Round(Math.Sin(a) * 255);
                                    }
                                    else
                                    {
                                        r = (byte)Math.Round(Math.Sin(a) * 255);
                                        g = (byte)Math.Round(Math.Cos(a) * 255);
                                        b = 0;
                                    }

                                    for (int sy = 0; sy < scale; ++sy)
                                    {
                                        for (int sx = 0; sx < scale; ++sx)
                                        {
                                            int offset = (y1 * sizeY2 * scale + y2 * scale + sy) * fb.RowBytes + (x1 * sizeX2 * scale + x2 * scale + sx) * 4;
                                            adr[offset] = b; // blue
                                            adr[offset + 1] = g; // green
                                            adr[offset + 2] = r; // red
                                            adr[offset + 3] = 255; // alpha
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ImageSource = wb;
        }
    }
}
