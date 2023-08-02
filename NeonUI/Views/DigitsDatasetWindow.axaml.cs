using Avalonia.Controls;
using Avalonia.Interactivity;
using Common;
using NeonUI.ViewModels;

namespace NeonUI.Views
{
    public partial class DigitsDatasetWindow : Window
    {
        private readonly DigitsDatasetVM _vm;
        public DigitsDatasetWindow()
        {
            InitializeComponent();
            _vm = new DigitsDatasetVM();
            DataContext = _vm;
            _vm.CloseWindow = Close;
        }

        public void Initialize(IDataset ds)
        {
            _vm.Initialize(ds);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            _vm.Close();
        }
    }
}
