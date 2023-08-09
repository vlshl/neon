using Avalonia.Controls;
using Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using NeonUI.Views;
using Avalonia.Platform.Storage;

namespace NeonUI.ViewModels;

public class MainVM : WindowViewModel
{
    public ICommand NewDatasetCommand { get; set; }
    public ICommand OpenDatasetCommand { get; set; }
    public ICommand NewNeuronetCommand { get; set; }
    public ICommand OpenNeuronetCommand { get; set; }
    public ICommand ExitCommand { get; set; }

    public IMessagePanel? MessagePanel { get; set; }

    public MainVM()
    {
        NewDatasetCommand = ReactiveCommand.Create(() => NewDataset());
        OpenDatasetCommand = ReactiveCommand.Create(() => OpenDataset());

        NewNeuronetCommand = ReactiveCommand.Create(() => NewNeuronet());
        OpenNeuronetCommand = ReactiveCommand.Create(() => OpenNeuronet());

        ExitCommand = ReactiveCommand.Create(() => { CloseWindowAction?.Invoke(); });
        MessagePanel = null;
    }

    private void NewDataset()
    {
        var dlg = new NewMnistDatasetDialog();
        dlg.ShowDialog(MainWindow.Instance);
    }

    private async void OpenDataset()
    {
        try
        {
            var sp = MainWindow.Instance.StorageProvider;
            var fileType = new FilePickerFileType("ds")
            { 
                Patterns = new string[] { "*." + DatasetManager.Instance.GetDsFileExt() }
            };
            IStorageFolder? dir = await sp.TryGetFolderFromPathAsync(DatasetManager.Instance.GetDatasetDirectory());
            if (dir == null) return;

            var files = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Набор данных",
                AllowMultiple = false, 
                FileTypeFilter = new[] { fileType }, 
                SuggestedStartLocation = dir
            });
            if (!files.Any()) return;

            var path = files.First().Path.AbsolutePath;
            bool isOpened = DatasetManager.Instance.IsDatasetOpened(path);
            if (isOpened)
            {
                MessagePanel?.ShowMessage("Набор данных уже открыт");
                return;
            }

            bool isSuccess = DatasetManager.Instance.OpenDataset(path);
            if (isSuccess)
            {
                OpenDatasetWindow(path);
            }
            else
            {
                MessagePanel?.ShowMessage("Ошибка при открытии набора данных");
            }
        }
        catch(Exception ex)
        {
            MessagePanel?.ShowException(ex);
        }
    }

    private void OpenDatasetWindow(string path)
    {
        var ds = DatasetManager.Instance.GetDataset(path);
        if (ds == null) return;

        var win = new MnistDatasetWindow();
        win.Initialize(ds);
        win.Show(MainWindow.Instance);
    }

    private void NewNeuronet()
    {
        var dlg = new NewPerzNeuronetDialog();
        dlg.ShowDialog(MainWindow.Instance);
    }

    private async void OpenNeuronet()
    {
        var sp = MainWindow.Instance.StorageProvider;
        var fileType = new FilePickerFileType("nn")
        {
            Patterns = new string[] { "*." + NeuronetManager.Instance.GetNeuronetFileExt() }
        };
        IStorageFolder? dir = await sp.TryGetFolderFromPathAsync(NeuronetManager.Instance.GetNeuronetDirectory());
        if (dir == null) return;

        var files = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Сеть",
            AllowMultiple = false,
            FileTypeFilter = new[] { fileType },
            SuggestedStartLocation = dir
        });
        if (!files.Any()) return;

        var filepath = files.First().Path.AbsolutePath;
        
        bool isOpened = NeuronetManager.Instance.IsNeuronetOpened(filepath);
        if (isOpened)
        {
            MessagePanel?.ShowMessage("Нейросеть уже открыта");
            return;
        }

        bool isSuccess = NeuronetManager.Instance.OpenNeuronet(filepath);
        if (isSuccess)
        {
            OpenNeuronetWindow(filepath);
        }
        else
        {
            MessagePanel?.ShowMessage("Ошибка при открытии нейросети");
        }
    }

    private void OpenNeuronetWindow(string path)
    {
        var nn = NeuronetManager.Instance.GetNeuronet(path);
        if (nn == null) return;

        var win = new PerzNeuronetWindow();
        win.Initialize(nn);
        win.Show(MainWindow.Instance);
    }
}