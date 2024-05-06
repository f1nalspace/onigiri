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
        /// Do not save anything, just load from all sources.
        /// </summary>
        ReadOnly = 1 << 0,

        /// <summary>
        /// Overwrite the details XML in the local store.
        /// </summary>
        ForceDetails = 1 << 1,

        /// <summary>
        /// Overwrite the images in the local store.
        /// </summary>
        ForcePicture = 1 << 2,

        /// <summary>
        /// Download the details XML to the local store.
        /// </summary>
        DownloadDetails = 1 << 3,

        /// <summary>
        /// Download the images to the local store.
        /// </summary>
        DownloadPicture = 1 << 4,

        /// <summary>
        /// Download the titles XML, without touching the persistent cache or the local store.
        /// </summary>
        DownloadTitles = 1 << 5,

        /// <summary>
        /// Parses the media files and extracts the meta data, such as video size, bitrate, etc.
        /// </summary>
        ParseMediaInfo = 1 << 6,
    }
}
