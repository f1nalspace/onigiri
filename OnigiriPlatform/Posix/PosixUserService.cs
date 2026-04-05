using Finalspace.Onigiri.Security;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Posix;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
[SupportedOSPlatform(nameof(OSPlatform.FreeBSD))]
sealed class PosixUserService : IUserService
{
    public IUserIdentity GetCurrentUser() => PosixUserIdentity.Current();
    
    public IImpersonationContext Impersonate(IUserIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (identity is not PosixUserIdentity linuxIdentity)
            throw new ArgumentException("Invalid linux user identity type", nameof(identity));
        return new PosixImpersonationContext(linuxIdentity);
    }
}
