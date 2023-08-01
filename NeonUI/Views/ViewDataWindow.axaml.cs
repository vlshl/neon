using Avalonia.Controls;
using Common;
using NeonUI.ViewModels;

namespace NeonUI.Views
{
    public partial class ViewDataWindow : Window
    {
        private readonly ViewDataVM _vm;
 
        public ViewDataWindow()
        {
            InitializeComponent();
            _vm = new ViewDataVM();
            DataContext = _vm;
        }

        public void Initialize(INetwork net, string dataKey)
        {
            _vm.Initialize(net, dataKey);
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            _vm.Close();
        }
    }
}
