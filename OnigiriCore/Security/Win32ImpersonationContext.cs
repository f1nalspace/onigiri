using System;

namespace Finalspace.Onigiri.Security
{
    class Win32ImpersonationContext : IImpersonationContext
    {
        private readonly IUserIdentity _identity;

        public Win32ImpersonationContext(IUserIdentity identity)
        {
            _identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        public void Dispose()
        {
        }
    }
}
