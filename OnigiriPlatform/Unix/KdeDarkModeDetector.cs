using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace Finalspace.Onigiri.Unix;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("freebsd")]
sealed class KdeDarkModeDetector : IDarkModeDetector
{
    private static readonly Regex DarkThemeRegex =
        new(@"dark|breeze-dark|night", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool IsAvailable =>
        IsKdeDesktop();

    public bool IsDarkMode 
    {
        get => _isDarkMode ??= ReadCurrentState();
        private set => _isDarkMode = value; 
    }
    private bool? _isDarkMode = null;

    public KdeDarkModeDetector()
    {
        _isDarkMode = null;
    }

    private static bool ReadCurrentState()
    {
        // Prefer KDE config/source of truth when available.
        // This implementation uses the currently active GTK/Qt-style theme hints
        // as a pragmatic approach that works well enough as a first pass.

        string kdeGlobals = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".config",
            "kdeglobals");

        if (File.Exists(kdeGlobals))
        {
            string text = File.ReadAllText(kdeGlobals);
            return DarkThemeRegex.IsMatch(text);
        }

        // Fallback: inspect common KDE-related environment hints
        string lookAndFeel = Environment.GetEnvironmentVariable("KDE_COLOR_SCHEME")
                             ?? Environment.GetEnvironmentVariable("PLASMA_STYLE")
                             ?? string.Empty;

        return DarkThemeRegex.IsMatch(lookAndFeel);
    }

    private static bool IsKdeDesktop()
    {
        string currentDesktop = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") ?? string.Empty;
        string session = Environment.GetEnvironmentVariable("DESKTOP_SESSION") ?? string.Empty;
        string fullSession = Environment.GetEnvironmentVariable("KDE_FULL_SESSION") ?? string.Empty;

        return currentDesktop.Contains("KDE", StringComparison.OrdinalIgnoreCase)
               || currentDesktop.Contains("PLASMA", StringComparison.OrdinalIgnoreCase)
               || session.Contains("plasma", StringComparison.OrdinalIgnoreCase)
               || string.Equals(fullSession, "true", StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
    }
}
