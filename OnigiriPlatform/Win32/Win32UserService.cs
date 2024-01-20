using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Security
{
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
    class Win32UserService : IUserService
    {
        public IUserIdentity GetCurrentUser() => new Win32UserIdentity();

        public IImpersonationContext Impersonate(IUserIdentity identity)
        {
            return new Win32ImpersonationContext(identity);
        }
    }
}
