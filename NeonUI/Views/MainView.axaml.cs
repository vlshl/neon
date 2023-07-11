using Avalonia.Controls;

namespace NeonUI.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        public IMessagePanel MessagePanel { get { return message; } }
    }
}
