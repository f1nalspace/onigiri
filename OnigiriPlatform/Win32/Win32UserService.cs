using Finalspace.Onigiri.Security;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Win32;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
sealed class Win32UserService : IUserService
{
    public IUserIdentity GetCurrentUser() => Win32UserIdentity.Current();

    public IImpersonationContext Impersonate(IUserIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (identity is not Win32UserIdentity winIdentity)
            throw new ArgumentException("Invalid windows user identity type", nameof(identity));
        return new Win32ImpersonationContext(winIdentity);
    }
}