using Avalonia.Controls;
using Avalonia.Interactivity;
using Common;
using UI.ViewModels;

namespace UI.Views
{
    public partial class PerzNeuronetWindow : Window
    {
        private readonly PerzNeuronetVM _vm;

        public PerzNeuronetWindow()
        {
            InitializeComponent();
            _vm = new PerzNeuronetVM();
            DataContext = _vm;
        }

        public void Initialize(INetwork nn)
        {
            _vm.Initialize(nn);
        }
    }
}
