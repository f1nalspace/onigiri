using System;

namespace Finalspace.Onigiri;

public interface IDarkModeDetector : IDisposable
{
    bool IsAvailable { get; }
    bool IsDarkMode { get; }
}
