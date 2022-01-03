using System;

namespace Finalspace.Onigiri.Services
{
    interface IDarkModeDetectionService
    {
        event EventHandler<bool> DarkModeChanged;
        bool IsDarkMode { get; }
    }
}
