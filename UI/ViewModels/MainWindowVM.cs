using Avalonia.Controls;
using Core;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using UI.Views;

namespace UI.ViewModels
{
    public class MainWindowVM : ViewModelBase
    {
        public ICommand NewDatasetCommand { get; set; }
        public ICommand OpenDatasetCommand { get; set; }
        public ICommand NewNeuronetCommand { get; set; }
        public ICommand OpenNeuronetCommand { get; set; }

        public ObservableCollection<MenuItemVM> DatasetItems { get; set; }
        public ObservableCollection<MenuItemVM> NeuronetItems { get; set; }

        public MainWindowVM()
        {
            NewDatasetCommand = ReactiveCommand.Create(() => NewDataset());
            OpenDatasetCommand = ReactiveCommand.Create(() => OpenDataset());
            DatasetItems = new ObservableCollection<MenuItemVM>();

            NewNeuronetCommand = ReactiveCommand.Create(() => NewNeuronet());
            OpenNeuronetCommand = ReactiveCommand.Create(() => OpenNeuronet());
            NeuronetItems = new ObservableCollection<MenuItemVM>();
        }

        private void NewDataset()
        {
            var dlg = new NewDigitsDatasetDialog();
            dlg.ShowDialog(MainWindow.Instance);
        }

        private async void OpenDataset()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Directory = DatasetManager.Instance.GetDatasetDirectory();
            var f = new FileDialogFilter();
            f.Extensions.Add(DatasetManager.Instance.GetDsFileExt());
            ofd.Filters.Add(f);
            var res = await ofd.ShowAsync(MainWindow.Instance);
            if ((res == null) || !res.Any()) return;

            string filepath = res.First();
            var name = DatasetManager.Instance.OpenDataset(filepath);
            if (string.IsNullOrEmpty(name)) return;
            OpenDataset(name);

            // добавить в список
            DatasetItems.Clear();
            string[] items = DatasetManager.Instance.GetDatasets();
            foreach (string item in items)
            {
                DatasetItems.Add(new MenuItemVM() { Header = item, Command = ReactiveCommand.Create(() => OpenDataset(item)) });
            }
        }

        private void OpenDataset(string name)
        {
            var ds = DatasetManager.Instance.GetDataset(name);
            if (ds == null) return;

            var win = new DigitsDatasetWindow();
            win.Initialize(ds);
            win.Show(MainWindow.Instance);
        }

        private void NewNeuronet()
        {
            var dlg = new NewPerzNeuronetDialog();
            dlg.ShowDialog(MainWindow.Instance);
        }

        private async void OpenNeuronet()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Directory = NetworkManager.Instance.GetNeuronetDirectory();
            var f = new FileDialogFilter();
            f.Extensions.Add(NetworkManager.Instance.GetNeuronetFileExt());
            ofd.Filters.Add(f);
            var res = await ofd.ShowAsync(MainWindow.Instance);
            if ((res == null) || !res.Any()) return;

            string filepath = res.First();
            var name = NetworkManager.Instance.OpenNetwork(filepath);
            if (string.IsNullOrEmpty(name)) return;
            OpenNeuronet(name);

            // добавить в список
            NeuronetItems.Clear();
            string[] items = NetworkManager.Instance.GetNetworks();
            foreach (string item in items)
            {
                NeuronetItems.Add(new MenuItemVM() { Header = item, Command = ReactiveCommand.Create(() => OpenNeuronet(item)) });
            }
        }

        private void OpenNeuronet(string name)
        {
            var nn = NetworkManager.Instance.GetNetwork(name);
            if (nn == null) return;

            var win = new PerzNeuronetWindow();
            win.Initialize(nn);
            win.Show(MainWindow.Instance);
        }
    }

}