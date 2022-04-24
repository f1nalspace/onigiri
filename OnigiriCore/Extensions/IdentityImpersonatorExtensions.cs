using Finalspace.Onigiri.Security;
using System;

namespace Finalspace.Onigiri.Extensions
{
    static class IdentityImpersonatorExtensions
    {
        public static IImpersonationContext Impersonate(this IIdentityImpersonator impersonator, IUserIdentity identity)
        {
            if (impersonator == null)
                throw new ArgumentNullException(nameof(impersonator));
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));
            IImpersonationContext result = impersonator.Impersonate(identity);
            return (result);
        }
    }
}
