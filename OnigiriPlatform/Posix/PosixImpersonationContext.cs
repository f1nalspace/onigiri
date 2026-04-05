using Finalspace.Onigiri.Security;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Posix;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
[SupportedOSPlatform(nameof(OSPlatform.FreeBSD))]
sealed class PosixImpersonationContext : IImpersonationContext
{
    private readonly PosixUserIdentity _identity;

    public PosixImpersonationContext(PosixUserIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identity = identity;
    }
    
    public void Dispose()
    {
    }
}
