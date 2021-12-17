using Finalspace.Onigiri.MVVM;
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
            ServiceContainer.Instance.Register<IProcessStarterService>(new DefaultProcessStarterService());
        }
    }
}
