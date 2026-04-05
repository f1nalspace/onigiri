using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Finalspace.Onigiri.Posix;

[SupportedOSPlatform(nameof(OSPlatform.Linux))]
[SupportedOSPlatform(nameof(OSPlatform.FreeBSD))]
sealed class PosixUserIdentity : IUserIdentity
{
    public string UserName { get; }

    public PosixUserIdentity(string userName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        UserName = userName;
    }

    public static PosixUserIdentity Current()
        => new PosixUserIdentity(Environment.UserName);

    public override string ToString() => UserName;

    private volatile bool _disposed = false;

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
        {
        }
        _disposed = true;
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
