using Finalspace.Onigiri.Security;
using Finalspace.Onigiri.Win32;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
sealed class LinuxUserService : IUserService
{
    public IUserIdentity GetCurrentUser() => LinuxUserIdentity.Current();
    
    public IImpersonationContext Impersonate(IUserIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (identity is not LinuxUserIdentity linuxIdentity)
            throw new ArgumentException("Invalid linux user identity type", nameof(identity));
        return new LinuxImpersonationContext(linuxIdentity);
    }
}
