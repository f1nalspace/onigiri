using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Services;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("freebsd")]
sealed class GnomeDarkModeDetector : IDarkModeDetector
{
    public bool IsAvailable =>
        IsGnomeDesktop();

    public bool IsDarkMode { get => _isDarkMode; private set => _isDarkMode = value; }
    private bool _isDarkMode = false;

    private static bool IsGnomeDesktop()
    {
        string currentDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") ?? string.Empty;
        string session = Environment.GetEnvironmentVariable("DESKTOP_SESSION") ?? string.Empty;

        return currentDesktop.Contains("GNOME", StringComparison.OrdinalIgnoreCase)
               || session.Contains("gnome", StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
    }
}
