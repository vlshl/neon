using Core;
using DigitsDs;

namespace UI.ViewModels
{
    public class NewDigitsDatasetVM : DialogViewModel
    {
        public NewDigitsDatasetVM() 
        { 
            Name = string.Empty;
            Path = string.Empty;
            ImagesFilename = string.Empty;
            LabelsFilename = string.Empty;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public string ImagesFilename { get; set; }
        public string LabelsFilename { get; set; }

        public override void OnOk()
        {
            if (string.IsNullOrEmpty(Name))
            {
                MessagePanel?.ShowMessage("Не введено наименование");
                return;
            }

            if (string.IsNullOrEmpty(Path))
            {
                MessagePanel?.ShowMessage("Не введен каталог с файлами набора данных");
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

            DigitsDatasetSettings settings = new DigitsDatasetSettings();
            settings.Dir = Path;
            settings.ImagesFilename = ImagesFilename;
            settings.LabelsFilename = LabelsFilename;
            bool isSuccess = DatasetManager.Instance.CreateDataset(Name.Trim(), settings);

            if (isSuccess)
            {
                base.OnOk();
            }
            else
            {
                MessagePanel?.ShowMessage("Ошибка при создании набора данных");
            }
        }
    }
}
