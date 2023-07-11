using Avalonia.Controls;
using NeonUI.ViewModels;

namespace NeonUI.Views
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
