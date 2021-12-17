using System;

namespace Finalspace.Onigiri.Enums
{
    [Flags]
    public enum UpdateFlags
    {
        None = 0,
        ForceDetails = 1 << 0,
        ForcePicture = 1 << 1,
        DownloadDetails = 1 << 2,
        DownloadPicture = 1 << 3,
        WriteCache = 1 << 4,
        DownloadTitles = 1 << 5,
    }
}
