using System;

namespace Finalspace.Onigiri.Services;

public interface IDarkModeDetectionService
{
    event EventHandler<bool> DarkModeChanged;
    bool IsDarkMode { get; }
}
