using DevExpress.Mvvm;
using Finalspace.Onigiri.Services;
using log4net.Config;
using System.Windows;

namespace Finalspace.Onigiri
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            XmlConfigurator.Configure();
            ServiceContainer.Default.RegisterService(new DefaultProcessStarterService());
            ServiceContainer.Default.RegisterService(new Win32DarkModeDetectionService());
        }
    }
}
