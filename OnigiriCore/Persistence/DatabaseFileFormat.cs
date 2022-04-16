using Finalspace.Onigiri.Utils;
using System;
using System.Runtime.InteropServices;

namespace Finalspace.Onigiri.Persistence
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct DatabaseFileHeader
    {
        public static uint MagicKey = 3309328592; // CRC32 hash from ONIGIRI_PERSISTENCE_DATABASE
        public static uint CurrentVersion = (uint)DatabaseFileVersion.Initial;

        public uint Magic;
        public uint Version;
        public uint IsReadonly;

        public ulong TableSize;
        public ulong EntryCount;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct DatabaseFileTableEntry
    {
        public ulong Aid;
        public ulong Offset;
        public ulong Size; // Data size without entry header
        public FourCC Type;
        public FourCC Format;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    struct DatabaseFileEntryHeader
    {
        public uint CRC32; // CRC32 hash from Table entry
        public DateTime Date; // Stored in UTC
        public uint Reserved0;
        public uint Reserved1;
        // ... the byte data
    }
}
