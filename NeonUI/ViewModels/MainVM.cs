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

    public ObservableCollection<MenuItem> DatasetItems { get; set; }
    public ObservableCollection<MenuItem> NeuronetItems { get; set; }

    public IMessagePanel? MessagePanel { get; set; }

    public MainVM()
    {
        NewDatasetCommand = ReactiveCommand.Create(() => NewDataset());
        OpenDatasetCommand = ReactiveCommand.Create(() => OpenDataset());
        DatasetItems = new ObservableCollection<MenuItem>();

        NewNeuronetCommand = ReactiveCommand.Create(() => NewNeuronet());
        OpenNeuronetCommand = ReactiveCommand.Create(() => OpenNeuronet());
        NeuronetItems = new ObservableCollection<MenuItem>();

        ExitCommand = ReactiveCommand.Create(() => { CloseWindow?.Invoke(); });
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
            bool isSuccess = DatasetManager.Instance.OpenDataset(path);
            if (!isSuccess) return;

            OpenDataset(path);
            RefreshDsItems();
        }
        catch(Exception ex)
        {
            MessagePanel?.ShowException(ex);
        }
    }

    public void RefreshDsItems()
    {
        DatasetItems.Clear();
        string[] paths = DatasetManager.Instance.GetDatasetPaths();
        foreach (string path in paths)
        {
            string name = DatasetManager.Instance.GetDatasetName(path);
            DatasetItems.Add(new MenuItem() { Header = name, Command = ReactiveCommand.Create(() => OpenDataset(path)) });
        }
    }

    private void OpenDataset(string path)
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
        bool isSuccess = NeuronetManager.Instance.OpenNeuronet(filepath);
        if (!isSuccess) return;

        OpenNeuronet1(filepath);
        RefreshNnItems();
    }

    private void OpenNeuronet1(string path)
    {
        var nn = NeuronetManager.Instance.GetNeuronet(path);
        if (nn == null) return;

        var win = new PerzNeuronetWindow();
        win.Initialize(nn);
        win.Show(MainWindow.Instance);
    }

    public void RefreshNnItems()
    {
        NeuronetItems.Clear();
        string[] paths = NeuronetManager.Instance.GetNeuronetPaths();
        foreach (string path in paths)
        {
            string name = NeuronetManager.Instance.GetNeuronetName(path);
            NeuronetItems.Add(new MenuItem() { Header = name, Command = ReactiveCommand.Create(() => OpenNeuronet1(path)) });
        }
    }
}