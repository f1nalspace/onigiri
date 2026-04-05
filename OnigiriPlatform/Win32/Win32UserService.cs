using Finalspace.Onigiri.Security;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Win32;

[SupportedOSPlatform(nameof(OSPlatform.Windows))]
class Win32UserService : IUserService
{
    public IUserIdentity GetCurrentUser() => new Win32UserIdentity();

    public IImpersonationContext Impersonate(IUserIdentity identity)
    {
        return new Win32ImpersonationContext(identity);
    }
}