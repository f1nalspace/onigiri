using Finalspace.Onigiri.Security;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Win32;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
sealed class Win32ImpersonationContext : IImpersonationContext
{
    private readonly Win32UserIdentity _identity;

    public Win32ImpersonationContext(Win32UserIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identity = identity;
    }

    public void Dispose()
    {
    }
}