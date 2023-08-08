using Avalonia.Platform.Storage;
using Core;
using Dataset;
using NeonUI.Views;
using ReactiveUI;
using System;
using System.Linq;
using System.Windows.Input;

namespace NeonUI.ViewModels;

public class NewMnistDatasetVM : DialogViewModel
{
    public ICommand SelectFolderCommand { get; set; }
    public ICommand SelectImagesFileCommand { get; set; }
    public ICommand SelectLabelsFileCommand { get; set; }

    public string Name { get; set; }
    public string FileName { get; set; }
    public string FolderPath { get => _folderPath; set => this.RaiseAndSetIfChanged(ref _folderPath, value); }
    public string ImagesFilename { get => _imagesFilename; set => this.RaiseAndSetIfChanged(ref _imagesFilename, value); }
    public string LabelsFilename { get => _labelsFilename; set => this.RaiseAndSetIfChanged(ref _labelsFilename, value); }

    private string _folderPath, _imagesFilename, _labelsFilename;

    public NewMnistDatasetVM()
    {
        Name = string.Empty;
        FileName = string.Empty;
        _folderPath = string.Empty;
        _imagesFilename = string.Empty;
        _labelsFilename = string.Empty;

        SelectFolderCommand = ReactiveCommand.Create(SelectFolder);
        SelectImagesFileCommand = ReactiveCommand.Create(SelectImagesFile);
        SelectLabelsFileCommand = ReactiveCommand.Create(SelectLabelsFile);
    }

    public override void OnOk()
    {
        if (string.IsNullOrEmpty(Name))
        {
            MessagePanel?.ShowMessage("Не введено наименование");
            return;
        }

        if (string.IsNullOrEmpty(FileName))
        {
            MessagePanel?.ShowMessage("Не введено наименование файла");
            return;
        }

        if (string.IsNullOrEmpty(FolderPath))
        {
            MessagePanel?.ShowMessage("Не выбран каталог с файлами данных");
            return;
        }

        if (string.IsNullOrEmpty(ImagesFilename))
        {
            MessagePanel?.ShowMessage("Не введено имя файла с образцами");
            return;
        }

        if (string.IsNullOrEmpty(LabelsFilename))
        {
            MessagePanel?.ShowMessage("Не введено имя файла с метками");
            return;
        }

        MnistSettings settings = new MnistSettings();
        settings.Name = Name;
        settings.FolderPath = FolderPath;
        settings.ImagesFilename = ImagesFilename;
        settings.LabelsFilename = LabelsFilename;
        
        try
        {
            bool isSuccess = DatasetManager.Instance.CreateDataset(FileName.Trim(), settings);

            if (isSuccess)
            {
                base.OnOk();
            }
            else
            {
                MessagePanel?.ShowMessage("Ошибка при создании источника данных");
            }
        }
        catch(Exception ex)
        {
            MessagePanel?.ShowException(ex);
        }
    }

    private async void SelectFolder()
    {
        var sp = MainWindow.Instance.StorageProvider;

        var folders = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Каталог файлов данных", 
            AllowMultiple = false
        });
        if (!folders.Any()) return;

        FolderPath = folders.First().Path.AbsolutePath;
    }

    private async void SelectImagesFile()
    {
        var sp = MainWindow.Instance.StorageProvider;
        IStorageFolder? dir = null;
        if (FolderPath.Any())
        {
            dir = await sp.TryGetFolderFromPathAsync(FolderPath);
        }

        var files = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Файл с образцами", 
            SuggestedStartLocation = dir, 
            AllowMultiple = false
        });
        if (!files.Any()) return;

        ImagesFilename = files.First().Name;
    }

    private async void SelectLabelsFile()
    {
        var sp = MainWindow.Instance.StorageProvider;
        IStorageFolder? dir = null;
        if (FolderPath.Any())
        {
            dir = await sp.TryGetFolderFromPathAsync(FolderPath);
        }

        var files = await sp.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Файл с метками",
            SuggestedStartLocation = dir,
            AllowMultiple = false
        });
        if (!files.Any()) return;

        LabelsFilename = files.First().Name;
    }
}
