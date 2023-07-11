using Avalonia.Controls;
using UI.ViewModels;

namespace UI.Views
{
    public partial class NewDigitsDatasetDialog : Window
    {
        public NewDigitsDatasetDialog()
        {
            InitializeComponent();

            var vm = new NewDigitsDatasetVM();
            vm.CloseDialog = CloseDialog;
            DataContext = vm;
            vm.MessagePanel = message;
        }

        public void CloseDialog(bool dialogResult)
        {
            this.Close(dialogResult);
        }
    }
}
