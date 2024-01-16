using Finalspace.Onigiri.ViewModels;
using Finalspace.Onigiri.Views;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Finalspace.Onigiri.Services
{
    class DefaultThemeManagerService : IThemeManagerService
    {
        public MainTheme CurrentTheme { get; private set; }

        private ResourceDictionary ThemeDictionary
        {
            get => Application.Current.Resources.MergedDictionaries[0];
            set => Application.Current.Resources.MergedDictionaries[0] = value;
        }

        private ResourceDictionary ControlsDictionary
        {
            get => Application.Current.Resources.MergedDictionaries[1];
            set => Application.Current.Resources.MergedDictionaries[1] = value;
        }

        private ResourceDictionary AppDictionary
        {
            get => Application.Current.Resources.MergedDictionaries[2];
            set => Application.Current.Resources.MergedDictionaries[2] = value;
        }

        public void ChangeTheme(MainTheme theme)
        {
            if (theme == MainTheme.Dark)
            {
                CurrentTheme = MainTheme.Dark;
                ThemeDictionary = new ResourceDictionary() { Source = new Uri($"Styles/DarkColors.xaml", UriKind.Relative) };
            }
            else
            {
                CurrentTheme = MainTheme.Light;
                ThemeDictionary = new ResourceDictionary() { Source = new Uri($"Styles/LightColors.xaml", UriKind.Relative) };
            }

            ControlsDictionary = new ResourceDictionary() { Source = new Uri($"Styles/Controls.xaml", UriKind.Relative) };
            AppDictionary = new ResourceDictionary() { Source = new Uri($"Styles/Onigiri.xaml", UriKind.Relative) };

            ResourceDictionary[] resourceDicts = Application.Current.Resources.MergedDictionaries.ToArray();

            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow.mainFrame.CanGoBack)
                mainWindow.mainFrame.GoBack();

            mainWindow.mainFrame.Navigate(new CardListPage());


            //if (mainWindow.mainFrame.Content is Page page)
            //{


            //    page.Resources.MergedDictionaries.Clear();
            //    foreach (ResourceDictionary resource in resourceDicts)
            //        page.Resources.MergedDictionaries.Add(resource);
            //}
        }
    }
}
