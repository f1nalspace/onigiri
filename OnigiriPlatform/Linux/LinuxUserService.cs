using Finalspace.Onigiri.Security;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Linux;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
sealed class LinuxUserService : IUserService
{
    public IUserIdentity GetCurrentUser()
        => LinuxUserIdentity.Current();
    
    public IImpersonationContext Impersonate(IUserIdentity identity)
        => throw new System.NotImplementedException();
}
