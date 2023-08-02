using Avalonia.Controls;
using Avalonia.Interactivity;
using System.IO;
using System;
using Core;
using NeonUI.ViewModels;

namespace NeonUI.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public IMessagePanel MessagePanel { get { return mainView.MessagePanel; } }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
        }
    }
}