using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using NeonUI.ViewModels;

namespace NeonUI.Views
{
    public interface IMessagePanel
    {
        void ShowMessage(string message);
        void ShowException(Exception ex);
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

        public void ShowException(Exception ex)
        {
            IsVisible = true;

            List<string> msgs = new List<string>();
            Exception? curEx = ex;
            while (curEx != null)
            {
                msgs.Add(curEx.Message);
                curEx = curEx.InnerException;
            }
            tbMessage.Text = string.Join('\n', msgs);
        }

        private void OnClose_Click(object sender, RoutedEventArgs e)
        {
            IsVisible = false;
        }
    }
}
