using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Core;
using NeonUI.ViewModels;
using NeonUI.Views;
using System.IO;

namespace NeonUI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mw = new MainWindow();
            var vm = new MainVM();
            mw.DataContext = vm;
            vm.MessagePanel = mw.MessagePanel;
            desktop.MainWindow = mw;
            vm.CloseWindow = () => { 
                desktop.Shutdown(); 
            };
        }
        //else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        //{
        //    var mv = new MainView();
        //    var vm = new MainVM();
        //    mv.DataContext = vm;
        //    vm.MessagePanel = mv.MessagePanel;
        //    singleViewPlatform.MainView = mv;
        //}

        base.OnFrameworkInitializationCompleted();
        OnAppOpen();
    }

    private void OnAppOpen()
    {
        string curDir = Directory.GetCurrentDirectory();
        string dbDir = Path.Combine(curDir, "database");
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
        NeuronetManager.Instance.Initialize(Path.GetFullPath(netDir));
    }
}
