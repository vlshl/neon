using Avalonia.Controls;
using Avalonia.Interactivity;
using Common;
using NeonUI.ViewModels;
using System.Threading;

namespace NeonUI.Views
{
    public partial class PerzNeuronetWindow : Window
    {
        private readonly PerzNeuronetVM _vm;

        public PerzNeuronetWindow()
        {
            InitializeComponent();
            _vm = new PerzNeuronetVM();
            DataContext = _vm;
            _vm.MessagePanel = message;
            _vm.CloseWindow = Close;
        }

        public void Initialize(INeuronet nn)
        {
            _vm.Initialize(nn);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            _vm.Close();
        }


    }
}
