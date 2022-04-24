using System;

namespace Finalspace.Onigiri.Enums
{
    [Flags]
    public enum UpdateFlags
    {
        /// <summary>
        /// No updates.
        /// </summary>
        None = 0,

        /// <summary>
        /// Overwrite the details XML in the local store.
        /// </summary>
        ForceDetails = 1 << 0,

        /// <summary>
        /// Overwrite the images in the local store.
        /// </summary>
        ForcePicture = 1 << 1,

        /// <summary>
        /// Download the details XML to the local store.
        /// </summary>
        DownloadDetails = 1 << 2,

        /// <summary>
        /// Download the images to the local store.
        /// </summary>
        DownloadPicture = 1 << 3,

        /// <summary>
        /// Download the titles XML, without touching the persistent cache or the local store.
        /// </summary>
        DownloadTitles = 1 << 4,
    }
}
