using Finalspace.Onigiri.Security;
using System.Security.Principal;

namespace Finalspace.Onigiri.Extensions
{
    static class WindowsIdentityExtensions
    {
        public static IImpersonationContext Impersonate(this WindowsIdentity identity)
        {
            Win32IdentityImpersonator impersonator = new Win32IdentityImpersonator();
            var result = impersonator.Impersonate(identity);
            return (result);
        }
    }
}
