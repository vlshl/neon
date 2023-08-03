using Core;
using Dataset;
using System;

namespace NeonUI.ViewModels;

public class NewMnistDatasetVM : DialogViewModel
{
    public NewMnistDatasetVM() 
    { 
        Name = string.Empty;
        Filename = string.Empty;
        FolderPath = string.Empty;
        ImagesFilename = string.Empty;
        LabelsFilename = string.Empty;
    }

    public string Name { get; set; }
    public string Filename { get; set; }
    public string FolderPath { get; set; }
    public string ImagesFilename { get; set; }
    public string LabelsFilename { get; set; }

    public override void OnOk()
    {
        if (string.IsNullOrEmpty(Name))
        {
            MessagePanel?.ShowMessage("Не введено наименование");
            return;
        }

        if (string.IsNullOrEmpty(Filename))
        {
            MessagePanel?.ShowMessage("Не введено наименование файла датасета");
            return;
        }

        if (string.IsNullOrEmpty(FolderPath))
        {
            MessagePanel?.ShowMessage("Не выбран каталог с файлами набора данных");
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
            bool isSuccess = DatasetManager.Instance.CreateDataset(Filename.Trim(), settings);

            if (isSuccess)
            {
                base.OnOk();
            }
            else
            {
                MessagePanel?.ShowMessage("Ошибка при создании набора данных");
            }
        }
        catch(Exception ex)
        {
            MessagePanel?.ShowException(ex);
        }
    }
}
