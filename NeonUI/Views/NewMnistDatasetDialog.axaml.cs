using Avalonia.Controls;
using NeonUI.ViewModels;

namespace NeonUI.Views
{
    public partial class NewMnistDatasetDialog : Window
    {
        public NewMnistDatasetDialog()
        {
            InitializeComponent();

            var vm = new NewMnistDatasetVM();
            vm.CloseDialog = CloseDialog;
            DataContext = vm;
            vm.MessagePanel = message;
        }

        private void CloseDialog(bool dialogResult)
        {
            this.Close(dialogResult);
        }
    }
}
