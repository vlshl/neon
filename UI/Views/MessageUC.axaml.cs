using Avalonia.Controls;
using Avalonia.Interactivity;
using UI.ViewModels;

namespace UI.Views
{
    public interface IMessagePanel
    {
        void ShowMessage(string message);
    }

    public partial class MessageUC : UserControl, IMessagePanel
    {
        public MessageUC()
        {
            InitializeComponent();
            IsVisible = false;
        }

        public void ShowMessage(string message)
        {
            IsVisible = true;
            tbMessage.Text = message;
        }

        private void OnClose_Click(object sender, RoutedEventArgs e)
        {
            IsVisible = false;
        }
    }
}
