using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Finalspace.Onigiri.Win32;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
sealed class Win32UserIdentity : IUserIdentity
{
    public string UserName => _identity?.User?.Value;

    private readonly WindowsIdentity _identity;

    public Win32UserIdentity(WindowsIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identity = identity;
    }
    
    public static IUserIdentity Current() => new Win32UserIdentity(WindowsIdentity.GetCurrent());

    private volatile bool _disposed = false;

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
            _identity.Dispose();
        _disposed = true;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}