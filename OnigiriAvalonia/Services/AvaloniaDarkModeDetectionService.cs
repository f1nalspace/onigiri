using Avalonia;
using Avalonia.Styling;
using System;

namespace Finalspace.Onigiri.Services;

class AvaloniaDarkModeDetectionService : IDarkModeDetectionService
{
    private readonly Application _app;

    public AvaloniaDarkModeDetectionService(Application app)
    {
        _app = app;
        _app.ActualThemeVariantChanged += OnThemeVariantChanged;
    }

    public event EventHandler<bool> DarkModeChanged;

    public bool IsDarkMode => _app.ActualThemeVariant == ThemeVariant.Dark;

    private void OnThemeVariantChanged(object sender, EventArgs e)
    {
        DarkModeChanged?.Invoke(this, IsDarkMode);
    }
}
