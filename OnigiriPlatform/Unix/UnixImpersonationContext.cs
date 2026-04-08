using Finalspace.Onigiri.Security;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Posix;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
[SupportedOSPlatform(nameof(OSPlatform.FreeBSD))]
sealed class UnixImpersonationContext : IImpersonationContext
{
    private readonly UnixUserIdentity _identity;

    public UnixImpersonationContext(UnixUserIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identity = identity;
    }
    
    public void Dispose()
    {
    }
}
