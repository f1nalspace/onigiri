using Finalspace.Onigiri.Security;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
sealed class LinuxImpersonationContext : IImpersonationContext
{
    private readonly LinuxUserIdentity _identity;

    public LinuxImpersonationContext(LinuxUserIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identity = identity;
    }
    
    public void Dispose()
    {
    }
}
