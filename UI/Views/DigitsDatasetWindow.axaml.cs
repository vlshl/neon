using Avalonia.Controls;
using Avalonia.Interactivity;
using Common;
using UI.ViewModels;

namespace UI.Views
{
    public partial class DigitsDatasetWindow : Window
    {
        private readonly DigitsDatasetVM _vm;
        public DigitsDatasetWindow()
        {
            InitializeComponent();
            _vm = new DigitsDatasetVM();
            DataContext = _vm;
        }

        public void Initialize(IDataset ds)
        {
            _vm.Initialize(ds);
        }
    }
}
