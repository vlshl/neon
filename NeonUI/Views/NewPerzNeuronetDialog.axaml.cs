using Avalonia.Controls;
using NeonUI.ViewModels;

namespace NeonUI.Views
{
    public partial class NewPerzNeuronetDialog : Window
    {
        public NewPerzNeuronetDialog()
        {
            InitializeComponent();
        
            var vm = new NewPerzNeuronetVM();
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
