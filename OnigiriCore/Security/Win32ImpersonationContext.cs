using System.Security.Principal;

namespace Finalspace.Onigiri.Security
{
    class Win32ImpersonationContext : IImpersonationContext
    {
        private readonly WindowsIdentity _identity;

        public Win32ImpersonationContext(WindowsIdentity identity)
        {
            _identity = identity;
        }

        public void Dispose()
        {
        }
    }
}
