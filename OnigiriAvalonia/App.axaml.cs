using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Finalspace.Onigiri.MVVM;
using Finalspace.Onigiri.Services;
using Finalspace.Onigiri.Views;
using Finalspace.Onigiri.ViewModels;
using Serilog;
using System.IO;

namespace Finalspace.Onigiri;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(OnigiriPaths.AppSettingsPath, "log_app.txt"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        ServiceContainer.Default.RegisterService<IProcessStarterService>(new DefaultProcessStarterService());
        ServiceContainer.Default.RegisterService<IThemeManagerService>(new AvaloniaThemeManagerService(this));
        ServiceContainer.Default.RegisterService<IDarkModeDetectionService>(new AvaloniaDarkModeDetectionService(this));

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();

            ServiceContainer.Default.RegisterService<IOnigiriDialogService>(
                new DefaultOnigiriDialogService(mainWindow));
            ServiceContainer.Default.RegisterService<IFileDialogService>(
                new AvaloniaFileDialogService(mainWindow));
            ServiceContainer.Default.RegisterService<IFolderDialogService>(
                new AvaloniaFolderDialogService(mainWindow));

            var mainViewModel = new MainViewModel();
            mainViewModel.CloseRequested += () => mainWindow.Close();
            mainWindow.DataContext = mainViewModel;
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
