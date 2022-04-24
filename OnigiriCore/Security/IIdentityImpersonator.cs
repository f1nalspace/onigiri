using System.Security.Principal;

namespace Finalspace.Onigiri.Security
{
    interface IIdentityImpersonator
    {
        IImpersonationContext Impersonate(IUserIdentity identity);
    }
}
