using Avalonia.Controls;
using Avalonia.Interactivity;
using System.IO;
using System;
using Core;
using UI.ViewModels;

namespace UI.Views
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            Instance = this;
            DataContext = new MainWindowVM();
        }

        public void miExit_Click(object sender, RoutedEventArgs args)
        {
            Close();
        }

        public void miOpen_Click(object sender, RoutedEventArgs args)
        {
            NewDigitsDatasetDialog dlg = new NewDigitsDatasetDialog();
            dlg.ShowDialog(this);
        }

        private void win_Opened(object sender, EventArgs args)
        {
            string curDir = Directory.GetCurrentDirectory();
            string dbDir = Path.Combine(curDir, "db");
            string dsDir = Path.Combine(dbDir, "datasets");
            string netDir = Path.Combine(dbDir, "nets");

            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            if (!Directory.Exists(dsDir))
            {
                Directory.CreateDirectory(dsDir);
            }
            DatasetManager.Instance.Initialize(Path.GetFullPath(dsDir));

            if (!Directory.Exists(netDir))
            {
                Directory.CreateDirectory(netDir);
            }
            NetworkManager.Instance.Initialize(Path.GetFullPath(netDir));
        }


    }
}