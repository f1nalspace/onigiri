namespace Finalspace.Onigiri.Security
{
    class Win32UserService : IUserService
    {
        public IUserIdentity GetCurrentUser() => new Win32UserIdentity();

        public IImpersonationContext Impersonate(IUserIdentity identity)
        {
            return new Win32ImpersonationContext(identity);
        }
    }
}
