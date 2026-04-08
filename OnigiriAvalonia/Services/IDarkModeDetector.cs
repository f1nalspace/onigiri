using System;

namespace Finalspace.Onigiri.Services;

public interface IDarkModeDetector : IDisposable
{
    bool IsAvailable { get; }
    bool IsDarkMode { get; }
}
