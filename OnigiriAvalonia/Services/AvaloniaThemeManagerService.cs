using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Finalspace.Onigiri.ViewModels;
using System;

namespace Finalspace.Onigiri.Services;

class AvaloniaThemeManagerService : IThemeManagerService
{
    private readonly Application _app;

    public AvaloniaThemeManagerService(Application app)
    {
        _app = app;
    }

    public MainTheme CurrentTheme { get; private set; }

    public void ChangeTheme(MainTheme theme)
    {
        CurrentTheme = theme;

        _app.RequestedThemeVariant = theme switch
        {
            MainTheme.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Light,
        };

        SwapColorDictionary(theme);
    }

    private void SwapColorDictionary(MainTheme theme)
    {
        string source = theme == MainTheme.Dark
            ? "avares://OnigiriAvalonia/Styles/DarkColors.axaml"
            : "avares://OnigiriAvalonia/Styles/LightColors.axaml";

        var uri = new Uri(source);
        var dict = _app.Resources.MergedDictionaries;
        if (dict.Count > 0)
            dict[0] = new ResourceInclude(uri) { Source = uri };
    }
}
