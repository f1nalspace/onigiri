namespace Finalspace.Onigiri.Security
{
    public interface IUserService
    {
        IUserIdentity GetCurrentUser();
        IImpersonationContext Impersonate(IUserIdentity identity);
    }
}
