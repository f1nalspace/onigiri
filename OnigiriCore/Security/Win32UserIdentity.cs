using System;
using System.Security.Principal;

namespace Finalspace.Onigiri.Security
{
    class Win32UserIdentity : IUserIdentity
    {
        public string UserName => _identity.User.Value;

        private readonly WindowsIdentity _identity;

        public Win32UserIdentity(WindowsIdentity identity = null)
        {
            _identity = identity ?? WindowsIdentity.GetCurrent();
        }
    }
}
