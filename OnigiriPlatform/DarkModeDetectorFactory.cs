using Finalspace.Onigiri.Posix;
using Finalspace.Onigiri.Unix;
using Finalspace.Onigiri.Win32;
using System;
using System.Runtime.InteropServices;

namespace Finalspace.Onigiri;

public interface IDarkModeDetectorFactory
{
    IDarkModeDetector Get();
}

public static class DarkModeDetectorFactory
{
    public static IDarkModeDetectorFactory Instance => _instance ??= new Impl();
    private static IDarkModeDetectorFactory _instance = null;

    private class Impl : IDarkModeDetectorFactory
    {
        public IDarkModeDetector Get()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new NotImplementedException();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                return new UnixDarkModeDetector();
            throw new PlatformNotSupportedException($"This Platform '{RuntimeInformation.OSDescription} {RuntimeInformation.ProcessArchitecture}' is not supported");
        }
    }
}
