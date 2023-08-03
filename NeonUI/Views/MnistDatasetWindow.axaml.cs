using Avalonia.Controls;
using Avalonia.Interactivity;
using Common;
using NeonUI.ViewModels;

namespace NeonUI.Views
{
    public partial class MnistDatasetWindow : Window
    {
        private readonly MnistDatasetVM _vm;
        public MnistDatasetWindow()
        {
            InitializeComponent();
            _vm = new MnistDatasetVM();
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
