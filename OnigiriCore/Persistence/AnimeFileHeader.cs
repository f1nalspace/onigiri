using System;
using System.Runtime.InteropServices;

namespace Finalspace.Onigiri.Persistence
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct AnimeFileHeader
    {
        public static ulong MagicKey = 0x8c450d2b; // CRC32 hash from ONIGIRI_PERSISTENCE
        public static ulong CurrentVersion = 1;

        public ulong Magic;
        public ulong Version;
        public ulong Aid;
        public ulong DetailsLength;
        public ulong PictureLength;
    }
}
