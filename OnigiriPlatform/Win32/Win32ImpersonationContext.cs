using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Security
{
    [SupportedOSPlatform(nameof(OSPlatform.Windows))]
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
