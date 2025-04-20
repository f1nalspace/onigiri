using Finalspace.Onigiri.Helpers;
using Finalspace.Onigiri.ViewModels;
using Finalspace.Onigiri.Views;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;

namespace Finalspace.Onigiri.Services
{
    class DefaultThemeManagerService : IThemeManagerService
    {
        public MainTheme CurrentTheme { get; private set; }

        private ResourceDictionary ThemeDictionary
        {
            get => Application.Current.Resources.MergedDictionaries[2];
            set => Application.Current.Resources.MergedDictionaries[2] = value;
        }

        public void ChangeTheme(MainTheme theme)
        {
            if (theme == MainTheme.Dark)
            {
                CurrentTheme = MainTheme.Dark;

                var paletteHelper = new PaletteHelper();
                Theme paletteTheme = paletteHelper.GetTheme();
                paletteTheme.SetBaseTheme(BaseTheme.Dark);
                paletteHelper.SetTheme(paletteTheme);

                ThemeDictionary = new ResourceDictionary() { Source = new Uri($"Styles/DarkColors.xaml", UriKind.Relative) };
            }
            else
            {
                CurrentTheme = MainTheme.Light;

                var paletteHelper = new PaletteHelper();
                Theme paletteTheme = paletteHelper.GetTheme();
                paletteTheme.SetBaseTheme(BaseTheme.Light);
                paletteHelper.SetTheme(paletteTheme);

                ThemeDictionary = new ResourceDictionary() { Source = new Uri($"Styles/LightColors.xaml", UriKind.Relative) };
            }

            foreach (Window window in Application.Current.Windows)
                WindowHelper.SetTheme(window, CurrentTheme);

            //
            // Reload frame, otherwise the style changes won't be applied
            //
            MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow.mainFrame.CanGoBack)
                mainWindow.mainFrame.GoBack();
            mainWindow.mainFrame.Navigate(new CardListPage());
        }
    }
}
