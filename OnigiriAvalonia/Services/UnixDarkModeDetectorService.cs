using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Services;

[SupportedOSPlatform("linux")]
[SupportedOSPlatform("freebsd")]
public sealed class UnixDarkModeDetectorService : IDarkModeDetector, IDisposable
{
    private readonly IReadOnlyCollection<IDarkModeDetector> _detectors;
    private readonly IDarkModeDetector _activeDetector;

    public UnixDarkModeDetectorService()
    {
        _detectors = [new KdeDarkModeDetector(), new GnomeDarkModeDetector()];
        _activeDetector = _detectors.FirstOrDefault(d => d.IsAvailable);
    }

    public bool IsAvailable => _activeDetector?.IsAvailable ?? false;
    public bool IsDarkMode => _activeDetector?.IsDarkMode ?? false;

    public void Dispose()
    {
        foreach (IDarkModeDetector detector in _detectors)
            detector.Dispose();
    }
}
