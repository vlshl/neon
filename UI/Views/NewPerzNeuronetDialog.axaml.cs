using Avalonia.Controls;
using UI.ViewModels;

namespace UI.Views
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
