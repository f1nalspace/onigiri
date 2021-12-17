using DevExpress.Mvvm;
using Finalspace.Onigiri.MVVM;
using Finalspace.Onigiri.Services;
using Finalspace.Onigiri.ViewModels;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Navigation;

namespace Finalspace.Onigiri.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ServiceContainer.Default.RegisterService(new DefaultOnigiriDialogService(this));
            DataContext = new MainViewModel();
            (DataContext as MainViewModel).CloseRequested += () => Close();
        }

        private void RibbonWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            (DataContext as MainViewModel).WindowLoaded();
        }

        private void UpdateFrameDataContext(object sender)
        {
            FrameworkElement content = mainFrame.Content as FrameworkElement;
            if (content == null)
                return;
            content.DataContext = mainFrame.DataContext;
        }

        private void mainFrame_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            UpdateFrameDataContext(sender);
        }

        private void mainFrame_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            UpdateFrameDataContext(sender);
        }
    }
}
