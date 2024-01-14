using System;

namespace Finalspace.Onigiri
{
    public interface IUserIdentity : IDisposable
    {
        string UserName { get; }
    }
}
