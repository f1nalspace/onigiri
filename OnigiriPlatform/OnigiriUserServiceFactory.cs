using Finalspace.Onigiri.Security;
using System;
using System.Runtime.InteropServices;

namespace Finalspace.Onigiri
{
    public interface IOnigiriUserServiceFactory
    {
        IUserService Create();
    }

    public static class OnigiriUserServiceFactory
    {
        public static IOnigiriUserServiceFactory Instance => _instance ??= new Impl();
        private static IOnigiriUserServiceFactory _instance = null;

        private class Impl : IOnigiriUserServiceFactory
        {
            public IUserService Create()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return new Win32UserService();
                else
                    throw new PlatformNotSupportedException($"This Platform '{RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture}' is not supported");
            }
        }
    }
}
