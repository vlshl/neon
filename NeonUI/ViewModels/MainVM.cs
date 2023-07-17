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

public class MainVM : ViewModelBase
{
    public ICommand NewDatasetCommand { get; set; }
    public ICommand OpenDatasetCommand { get; set; }
    public ICommand NewNeuronetCommand { get; set; }
    public ICommand OpenNeuronetCommand { get; set; }
    public ICommand ExitCommand { get; set; }

    public Action? OnExit { get; set; }

    public ObservableCollection<MenuItemVM> DatasetItems { get; set; }
    public ObservableCollection<MenuItemVM> NeuronetItems { get; set; }

    public IMessagePanel? MessagePanel { get; set; }

    public MainVM()
    {
        NewDatasetCommand = ReactiveCommand.Create(() => NewDataset());
        OpenDatasetCommand = ReactiveCommand.Create(() => OpenDataset());
        DatasetItems = new ObservableCollection<MenuItemVM>();

        NewNeuronetCommand = ReactiveCommand.Create(() => NewNeuronet());
        OpenNeuronetCommand = ReactiveCommand.Create(() => OpenNeuronet());
        NeuronetItems = new ObservableCollection<MenuItemVM>();

        ExitCommand = ReactiveCommand.Create(() => Exit());
        MessagePanel = null;
        OnExit = null;
    }

    private void NewDataset()
    {
        var dlg = new NewDigitsDatasetDialog();
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

            var filepath = files.First().Path.AbsolutePath;
            var name = DatasetManager.Instance.OpenDataset(filepath);
            if (string.IsNullOrEmpty(name)) return;

            OpenDataset(name);

            // добавить в список
            DatasetItems.Clear();
            string[] items = DatasetManager.Instance.GetDatasets();
            foreach (string item in items)
            {
                DatasetItems.Add(new MenuItemVM() { Header = item, Command = ReactiveCommand.Create(() => OpenDataset(item)) });
            }
        }
        catch(Exception ex)
        {
            MessagePanel?.ShowException(ex);
        }
    }

    private void OpenDataset(string name)
    {
        var ds = DatasetManager.Instance.GetDataset(name);
        if (ds == null) return;

        var win = new DigitsDatasetWindow();
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
            Patterns = new string[] { "*." + NetworkManager.Instance.GetNeuronetFileExt() }
        };
        IStorageFolder? dir = await sp.TryGetFolderFromPathAsync(NetworkManager.Instance.GetNeuronetDirectory());
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
        var name = NetworkManager.Instance.OpenNetwork(filepath);
        if (string.IsNullOrEmpty(name)) return;

        OpenNeuronet(name);

        // добавить в список
        NeuronetItems.Clear();
        string[] items = NetworkManager.Instance.GetNetworks();
        foreach (string item in items)
        {
            NeuronetItems.Add(new MenuItemVM() { Header = item, Command = ReactiveCommand.Create(() => OpenNeuronet(item)) });
        }
    }

    private void OpenNeuronet(string name)
    {
        var nn = NetworkManager.Instance.GetNetwork(name);
        if (nn == null) return;

        var win = new PerzNeuronetWindow();
        win.Initialize(nn);
        win.Show(MainWindow.Instance);
    }

    private void Exit()
    {
        OnExit?.Invoke();
    }
}