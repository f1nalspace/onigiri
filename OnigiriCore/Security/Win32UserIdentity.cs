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

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _identity.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
