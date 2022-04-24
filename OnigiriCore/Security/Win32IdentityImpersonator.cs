using System.Security.Principal;

namespace Finalspace.Onigiri.Security
{
    class Win32IdentityImpersonator : IIdentityImpersonator
    {
        public IImpersonationContext Impersonate(IUserIdentity identity)
        {
            return new Win32ImpersonationContext(identity);
        }
    }
}
