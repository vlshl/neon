using Avalonia;
using Avalonia.Media.Imaging;
using Common;
using Core;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace NeonUI.ViewModels
{
    public class ViewDataVM : WindowViewModel
    {
        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }

        public int ImageWidth { get => _imageWidth; set => this.RaiseAndSetIfChanged(ref _imageWidth, value); }
        public int ImageHeight { get => _imageHeight; set => this.RaiseAndSetIfChanged(ref _imageHeight, value); }
        public WriteableBitmap? ImageSource { get => _imageSource; set => this.RaiseAndSetIfChanged(ref _imageSource, value); }
        public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title, value); }

        private INeuronet? _net;
        private string _dataKey;
        private int _imageWidth;
        private int _imageHeight;
        private WriteableBitmap? _imageSource;
        private string _title;
        private int _scale;
        private int _sizeX1, _sizeY1, _sizeX2, _sizeY2;
        private double[,]? _data;

        public ViewDataVM()
        {
            _title = "";
            _dataKey = "";
            _scale = 1;
            _sizeX1 = _sizeY1 = _sizeX2 = _sizeY2 = 0;
            _data = null;

            ZoomInCommand = ReactiveCommand.Create(ZoomIn);
            ZoomOutCommand = ReactiveCommand.Create(ZoomOut);
        }

        public void Initialize(INeuronet net, string dataKey)
        {
            _net = net;
            _dataKey = dataKey;
            Refresh();
            NeuronetManager.Instance.OnNeuronetChange += OnNeuronetChange;
        }

        private void ZoomIn()
        {
            if (_scale >= 100) return;
            _scale++;
            ShowImage();
        }

        private void ZoomOut()
        {
            if (_scale <= 1) return;
            _scale--;
            ShowImage();
        }

        private void Refresh()
        {
            if (_net == null) return;

            _data = NeuronetManager.Instance.GetDataByKey(_net, _dataKey);
            if ((_data == null) || (_data.GetLength(0) == 0) || (_data.GetLength(1) == 0)) return;

            int arrSize1 = _data.GetLength(0);
            int arrSize2 = _data.GetLength(1);
            Title = _dataKey + " (" + arrSize1.ToString() + "x" + arrSize2.ToString() + ")";

            int s1 = (int)Math.Round(Math.Sqrt(arrSize1));
            if (s1 * s1 == arrSize1)
            {
                _sizeX1 = _sizeY1 = s1;
            }
            else
            {
                _sizeX1 = arrSize1; _sizeY1 = 1;
            }

            int s2 = (int)Math.Round(Math.Sqrt(arrSize2));
            if (s2 * s2 == arrSize2)
            {
                _sizeX2 = _sizeY2 = s2;
            }
            else
            {
                _sizeX2 = arrSize2; _sizeY2 = 1;
            }

            ShowImage();
        }

        private void OnNeuronetChange(INeuronet net, string e)
        {
            if (net == _net) Refresh();
        }

        public void Close()
        {
            NeuronetManager.Instance.OnNeuronetChange -= OnNeuronetChange;
        }

        private void ShowImage()
        {
            if (_data == null) return;

            int len1 = _data.GetLength(0);
            int len2 = _data.GetLength(1);
            if (len1 == 0 || len2 == 0) return;

            ImageWidth = _sizeX1 * _sizeX2 * _scale;
            ImageHeight = _sizeY1 * _sizeY2 * _scale;

            double? max_p = null;
            double? min_n = null;
            for (int i = 0; i < len1; ++i)
            {
                for (int j = 0; j < len2; ++j)
                {
                    double a = _data[i, j];
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

            WriteableBitmap wb = new WriteableBitmap(new PixelSize(_sizeX1 * _sizeX2 * _scale, _sizeY1 * _sizeY2 * _scale), new Vector(96, 96));

            using (var fb = wb.Lock())
            {
                unsafe
                {
                    byte* adr = (byte*)fb.Address;

                    for (int y1 = 0; y1 < _sizeY1; ++y1)
                    {
                        for (int x1 = 0; x1 < _sizeX1; ++x1)
                        {
                            int idx1 = y1 * _sizeX1 + x1;

                            for (int y2 = 0; y2 < _sizeY2; ++y2)
                            {
                                for (int x2 = 0; x2 < _sizeX2; ++x2)
                                {
                                    int idx2 = y2 * _sizeX2 + x2;
                                    if ((idx1 >= len1) || (idx2 >= len2)) continue;

                                    double a = _data[idx1, idx2];
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
                                        r = 0;
                                        g = (byte)Math.Round(d * 255);
                                        b = (byte)Math.Round(d * 255);
                                    }
                                    else
                                    {
                                        r = (byte)Math.Round(d * 255);
                                        g = 0;
                                        b = 0;
                                    }

                                    for (int sy = 0; sy < _scale; ++sy)
                                    {
                                        for (int sx = 0; sx < _scale; ++sx)
                                        {
                                            int offset = (y1 * _sizeY2 * _scale + y2 * _scale + sy) * fb.RowBytes + (x1 * _sizeX2 * _scale + x2 * _scale + sx) * 4;
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
