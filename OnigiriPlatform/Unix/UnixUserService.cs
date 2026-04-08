using Finalspace.Onigiri.Security;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Posix;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
[SupportedOSPlatform(nameof(OSPlatform.FreeBSD))]
sealed class UnixUserService : IUserService
{
    public IUserIdentity GetCurrentUser() => UnixUserIdentity.Current();
    
    public IImpersonationContext Impersonate(IUserIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (identity is not UnixUserIdentity linuxIdentity)
            throw new ArgumentException("Invalid linux user identity type", nameof(identity));
        return new UnixImpersonationContext(linuxIdentity);
    }
}
