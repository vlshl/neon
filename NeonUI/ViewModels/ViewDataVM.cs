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
        private INeuronet? _net;
        private string _dataKey;
        private int _imageWidth;
        private int _imageHeight;
        private int _winWidth;
        private int _winHeight;
        private WriteableBitmap? _imageSource;
        private string _title;
        private int _scale;

        public int ImageWidth { get => _imageWidth; set => this.RaiseAndSetIfChanged(ref _imageWidth, value); }
        public int ImageHeight { get => _imageHeight; set => this.RaiseAndSetIfChanged(ref _imageHeight, value); }
        public WriteableBitmap? ImageSource { get => _imageSource; set => this.RaiseAndSetIfChanged(ref _imageSource, value); }
        public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title, value); }
        public int WindowWidth { get => _winWidth; set => this.RaiseAndSetIfChanged(ref _winWidth, value); }
        public int WindowHeight { get => _winHeight; set => this.RaiseAndSetIfChanged(ref _winHeight, value); }

        public ViewDataVM()
        {
            _title = "";
            _dataKey = "";
            _winWidth = _winHeight = 100;
            _scale = 5;
        }

        public void Initialize(INeuronet net, string dataKey)
        {
            _net = net;
            _dataKey = dataKey;
            Refresh();
            NeuronetManager.Instance.OnNeuronetChange += OnNeuronetChange;
        }

        private void Refresh()
        {
            double[,] arr = NeuronetManager.Instance.GetDataByKey(_net, _dataKey);
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

            ShowImage(arr, sx1, sy1, sx2, sy2);
        }

        private void OnNeuronetChange(INeuronet net, string e)
        {
            if (net == _net) Refresh();
        }

        public void Close()
        {
            NeuronetManager.Instance.OnNeuronetChange -= OnNeuronetChange;
        }

        private void ShowImage(double[,] arr, int sizeX1, int sizeY1, int sizeX2, int sizeY2)
        {
            ImageWidth = sizeX1 * sizeX2 * _scale;
            ImageHeight = sizeY1 * sizeY2 * _scale;
            WindowWidth = ImageWidth + 50;
            WindowHeight = ImageHeight + 50;

            int len1 = arr.GetLength(0);
            int len2 = arr.GetLength(1);

            if (len1 == 0 || len2 == 0) return;
            double? max_p = null;
            double? min_n = null;
            for (int i = 0; i < len1; ++i)
            {
                for (int j = 0; j < len2; ++j)
                {
                    double a = arr[i, j];
                    if (a >= 0)
                    {
                        if (max_p != null)
                        {
                            if (a > max_p.Value) max_p = a;
                        }
                        else
                        {
                            max_p = a;
                        }
                    }
                    else
                    {
                        if (min_n != null)
                        {
                            if (a < min_n.Value) min_n = a;
                        }
                        else
                        {
                            min_n = a;
                        }
                    }
                }
            }

            WriteableBitmap wb = new WriteableBitmap(new PixelSize(sizeX1 * sizeX2 * _scale, sizeY1 * sizeY2 * _scale), new Vector(96, 96));

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

                                    double a = arr[idx1, idx2];
                                    double d = 0;
                                    if ((a >= 0) && (max_p != null))
                                    {
                                        d = Math.Abs(max_p.Value != 0 ? a / max_p.Value : 0);
                                    }
                                    else if ((a < 0) && (min_n != null))
                                    {
                                        d = Math.Abs(min_n.Value != 0 ? a / min_n.Value : 0);
                                    }

                                    byte r, g, b;
                                    if (a < 0)
                                    {
                                        r = (byte)Math.Round(d * 255);
                                        g = 0;
                                        b = 0;
                                    }
                                    else
                                    {
                                        r = 0;
                                        g = (byte)Math.Round(d * 255);
                                        b = 0;
                                    }

                                    for (int sy = 0; sy < _scale; ++sy)
                                    {
                                        for (int sx = 0; sx < _scale; ++sx)
                                        {
                                            int offset = (y1 * sizeY2 * _scale + y2 * _scale + sy) * fb.RowBytes + (x1 * sizeX2 * _scale + x2 * _scale + sx) * 4;
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
