using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Extensions;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.Types;
using Finalspace.Onigiri.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Finalspace.Onigiri.Storage
{
    public class DatabaseAnimeFilesStorage : IAnimeStorage
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        enum DatabaseFileVersion : uint
        {
            None = 0,
            Initial = 1,
            Latest = Initial,
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        struct DatabaseFileHeader
        {
            public static uint MagicKey = 3309328592; // CRC32 hash from ONIGIRI_PERSISTENCE_DATABASE

            public uint Magic;
            public uint Version;
            public uint EntryCount;
            public uint AnimeCount; // Just for validation
            public uint TitleCount; // Just for validation
            public uint Reserved;

            public ulong TableOffset;

            public static readonly uint RecordSize = (uint)Marshal.SizeOf(typeof(DatabaseFileHeader));
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        struct TitleIndex
        {
            public uint LanguageId;
            public uint TypeId;
            public uint TextId;
            public uint Reserved;

            public static readonly uint RecordSize = (uint)Marshal.SizeOf(typeof(TitleIndex));
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        struct TableEntry
        {
            public ulong Aid; // The anime id
            public ulong Offset; // The offset to the DatabaseFileEntryHeader
            public ulong Size; // The compressed data size
            public FourCC Type; // Type as FourCC
            public FourCC Format; // Format as FourCC

            public static readonly uint RecordSize = (uint)Marshal.SizeOf(typeof(TableEntry));
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        struct DataEntryHeader
        {
            public static uint MagicKey = 1440374961; // CRC32 hash from DatabaseFileEntryHeader

            public uint Magic; // The magic key for the file entry header
            public uint Hash; // Hash from uncompressed data
            public uint ID; // The id
            public FourCC Compression; // The compression format

            public DateTime Date; // Stored in UTC
            public ulong UmcompressedSize; // The uncompressed data size

            // ... the compressed byte data

            public static readonly uint RecordSize = (uint)Marshal.SizeOf(typeof(DataEntryHeader));
        }

        private static readonly FourCC DetailsEntryType = FourCC.FromString("DTLS");
        private static readonly FourCC PictureEntryType = FourCC.FromString("PICT");
        private static readonly FourCC TitleEntryType = FourCC.FromString("TITL");
        private static readonly FourCC StringEntryType = FourCC.FromString("STR_");

        private static readonly FourCC TextFormatType = FourCC.FromString("TEXT");
        private static readonly FourCC XMLFormatType = FourCC.FromString("XML_");
        private static readonly FourCC BinaryFormatType = FourCC.FromString("BINY");
        private static readonly FourCC LanguageFormatKind = FourCC.FromString("LANG");
        private static readonly FourCC TitleTypeFormatKind = FourCC.FromString("TTYP");

        private static readonly FourCC DeflateCompressionType = FourCC.FromString("DEFL");
        private static readonly FourCC GZipCompressionType = FourCC.FromString("GZIP");

        private readonly string _filePath;

        public DatabaseAnimeFilesStorage(string filePath)
        {
            _filePath = filePath;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TestHeader(DatabaseFileHeader header, string filePath, long fileLen)
        {
            if (header.Magic != DatabaseFileHeader.MagicKey ||
                header.EntryCount == uint.MaxValue ||
                header.AnimeCount == uint.MaxValue ||
                header.TitleCount == uint.MaxValue ||
                header.TableOffset == ulong.MaxValue)
                throw new FormatException($"Invalid header in database file '{filePath}'!");

            if (header.Version == 0 || header.Version == uint.MaxValue || header.Version > (uint)DatabaseFileVersion.Latest)
                throw new FormatException($"Header version of '{header.Version}' is not supported in database file '{filePath}'!");

            long lastTableOffset = (long)header.TableOffset + header.EntryCount * TableEntry.RecordSize;
            if (header.TableOffset == 0 || (long)header.TableOffset >= fileLen || lastTableOffset > fileLen)
                throw new FormatException($"Invalid table offset '{header.TableOffset}' in database file '{filePath}'!");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TestEntry(TableEntry entry, int entryIndex, long fileLen)
        {
            if (entry.Aid == 0 || entry.Aid >= uint.MaxValue)
                throw new FormatException($"Invalid aid of '{entry.Aid}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");

            if (entry.Type == FourCC.Empty)
                throw new FormatException($"Invalid type of '{entry.Type}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");
            if (!(entry.Type == DetailsEntryType || entry.Type == PictureEntryType || entry.Type == TitleEntryType))
                throw new FormatException($"Unsupported type of '{entry.Type}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");

            if (entry.Format == FourCC.Empty)
                throw new FormatException($"Invalid format of '{entry.Format}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");
            if (!(entry.Format == BinaryFormatType || entry.Format == XMLFormatType || entry.Format == TextFormatType))
                throw new FormatException($"Unsupported format of '{entry.Format}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");

            if (entry.Size == 0 || entry.Size >= uint.MaxValue)
                throw new FormatException($"Invalid size of '{entry.Size}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");

            if (entry.Offset == 0 || entry.Offset >= uint.MaxValue || entry.Offset > ((ulong)fileLen - entry.Size))
                throw new FormatException($"Invalid offset of '{entry.Offset}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TestFileEntry(TableEntry entry, int entryIndex, DataEntryHeader dataHeader)
        {
            if (dataHeader.Magic != DataEntryHeader.MagicKey)
                throw new FormatException($"Invalid magic for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");
            if (dataHeader.Compression != FourCC.Empty && (dataHeader.UmcompressedSize == 0 || dataHeader.UmcompressedSize == ulong.MaxValue))
                throw new FormatException($"Invalid uncompression size for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");
        }

        public bool Save(Anime anime, StatusChangedEventHandler statusChanged)
        {
            if (!File.Exists(_filePath))
            {
                Anime[] animes = new[] { anime };

                AnimeStorageData data = new AnimeStorageData(animes.ToImmutableArray(), ImmutableArray<Title>.Empty);

                bool result = Save(data, statusChanged);
                return result;
            }

            FileInfo fileInfo = new FileInfo(_filePath);
            long fileLen = fileInfo.Length;
            if (fileLen < DatabaseFileHeader.RecordSize)
                throw new FormatException($"Anime database file '{_filePath}' is too small!");

            using FileStream fileStream = new FileStream(_filePath, FileMode.Append, FileAccess.ReadWrite);

            DatabaseFileHeader header = fileStream.ReadStruct<DatabaseFileHeader>();

            TestHeader(header, _filePath, fileLen);

            fileStream.Seek((long)header.TableOffset, SeekOrigin.Begin);

            // - Load database file
            // - Load table entries into array
            // - Jump to table offset
            // - Serialize anime and write details and picture (Append a new entry for details and picture)
            // - Write all table entries
            // - Jump to the beginning and update header

            return false;
        }

        public bool Save(AnimeStorageData data, StatusChangedEventHandler statusChanged)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using FileStream fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write);

            // Write empty header first, so we can fill it out later
            fileStream.WriteStruct(new DatabaseFileHeader());

            ulong offset = DatabaseFileHeader.RecordSize;

            uint animeCount = (uint)data.Animes.Length;

            uint titleCount = (uint)data.Titles.Length;

            List<TableEntry> entries = new List<TableEntry>();

            int animeIdCounter = 0;
            int imageIdCounter = 0;

            // Write details & picture foreach
            int count = 0;
            int totalCount = data.Animes.Length;
            foreach (Anime anime in data.Animes)
            {
                int c = Interlocked.Increment(ref count);
                int percentage = (int)(c / (double)totalCount * 100.0);
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = anime.MainTitle, Percentage = percentage });

                {
                    ulong uncompressedSize;
                    MemoryStream compressedStream;
                    using (MemoryStream rawDetailsStream = AnimeSerialization.SerializeAnime(anime))
                    {
                        uncompressedSize = (ulong)rawDetailsStream.Length;
                        compressedStream = CompressionUtils.CompressGZip(rawDetailsStream);
                    }

                    ulong size = (ulong)compressedStream.Length;

                    // TODO(final): Implement CRC for anime details!
                    uint crc = 0;
                    // TODO(final): Read last update date from anime
                    DateTime date = DateTime.MinValue;
                    uint id = (uint)++animeIdCounter;

                    DataEntryHeader dataHeader = new DataEntryHeader()
                    {
                        Magic = DataEntryHeader.MagicKey,
                        ID = id,
                        Hash = crc,
                        Date = date,
                        UmcompressedSize = uncompressedSize,
                        Compression = GZipCompressionType,
                    };
                    fileStream.WriteStruct(dataHeader);

                    compressedStream.CopyTo(fileStream);

                    TableEntry entry = new TableEntry()
                    {
                        Aid = anime.Aid,
                        Offset = offset,
                        Size = size,
                        Format = XMLFormatType,
                        Type = DetailsEntryType,
                    };
                    entries.Add(entry);

                    offset += DataEntryHeader.RecordSize;
                    offset += size;
                }
                {
                    AnimeImage image = anime.Image;
                    if (image != null)
                    {
                        // TODO(final): Implement CRC for binary data (Picture)
                        uint crc = 0;
                        // TODO(final): Read creation date from anime image
                        DateTime date = DateTime.MinValue;

                        ulong size = (ulong)image.Data.Length;

                        uint id = (uint)++imageIdCounter;

                        DataEntryHeader dataHeader = new DataEntryHeader()
                        {
                            Magic = DataEntryHeader.MagicKey,
                            ID = id,
                            Hash = crc,
                            Date = date,
                            Compression = FourCC.Empty,
                            UmcompressedSize = size,
                        };
                        fileStream.WriteStruct(dataHeader);

                        fileStream.Write(image.Data.AsSpan());

                        TableEntry entry = new TableEntry()
                        {
                            Aid = anime.Aid,
                            Offset = offset,
                            Size = size,
                            Format = BinaryFormatType,
                            Type = PictureEntryType,
                        };
                        entries.Add(entry);

                        offset += DataEntryHeader.RecordSize;
                        offset += size;
                    }
                }
            }
            fileStream.Flush();

            // Write entries table
            ulong expectedTableOffset = (ulong)fileStream.Position;
            if (offset != expectedTableOffset)
                throw new InvalidOperationException($"Wrong table offset, expect '{expectedTableOffset}' but got '{offset}'!");
            ulong tableOffset = offset;
            foreach (TableEntry entry in entries)
                fileStream.WriteStruct(entry);
            fileStream.Flush();

            // Overwrite header
            DatabaseFileHeader header = new DatabaseFileHeader()
            {
                Magic = DatabaseFileHeader.MagicKey,
                EntryCount = (uint)entries.Count,
                AnimeCount = animeCount,
                TitleCount = titleCount,
                TableOffset = tableOffset,
                Version = (uint)DatabaseFileVersion.Latest,
            };
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.WriteStruct(header);
            fileStream.Flush();

            return true;
        }

        public AnimeStorageData Load(StatusChangedEventHandler statusChanged)
        {
            if (!File.Exists(_filePath))
                return null;

            FileInfo fileInfo = new FileInfo(_filePath);
            long fileLen = fileInfo.Length;
            if (fileLen < DatabaseFileHeader.RecordSize)
                throw new FormatException($"Anime database file '{_filePath}' is too small!");

            Dictionary<ulong, Anime> idToAnimeMap = new Dictionary<ulong, Anime>();

            using (FileStream fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                DatabaseFileHeader header = fileStream.ReadStruct<DatabaseFileHeader>();

                TestHeader(header, _filePath, fileLen);

                fileStream.Seek((long)header.TableOffset, SeekOrigin.Begin);

                TableEntry[] entries = new TableEntry[header.EntryCount];
                for (int i = 0; i < entries.Length; i++)
                    entries[i] = fileStream.ReadStruct<TableEntry>();

                // TODO(final): Parallel load!
                int entryIndex = 0;
                int entryCount = entries.Length;
                foreach (TableEntry entry in entries)
                {
                    TestEntry(entry, entryIndex, fileLen);

                    int c = entryIndex + 1;
                    int percentage = (int)(c / (double)entryCount * 100.0);
                    statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Entry {c} of {entryCount}", Percentage = percentage });

                    fileStream.Seek((long)entry.Offset, SeekOrigin.Begin);

                    DataEntryHeader dataHeader = fileStream.ReadStruct<DataEntryHeader>();

                    TestFileEntry(entry, entryIndex, dataHeader);

                    // TODO(final): Read in blocks of int32?
                    byte[] rawData = new byte[entry.Size];
                    int read = fileStream.Read(rawData, 0, rawData.Length);
                    if (read != (int)entry.Size)
                        throw new FormatException($"Failed to read data with length of '{entry.Size}' for entry {entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'");

                    MemoryStream dataStream;
                    if (dataHeader.Compression == FourCC.Empty)
                        dataStream = new MemoryStream(rawData);
                    else
                    {
                        if (dataHeader.Compression == GZipCompressionType)
                        {
                            using (MemoryStream compressedStream = new MemoryStream(rawData))
                                dataStream = CompressionUtils.DecompressGZip(compressedStream);
                        }
                        else if (dataHeader.Compression == DeflateCompressionType)
                        {
                            using (MemoryStream compressedStream = new MemoryStream(rawData))
                                dataStream = CompressionUtils.DecompressDeflate(compressedStream);
                        }
                        else
                            throw new NotSupportedException($"Compression type '{dataHeader.Compression}' is not supported for entry {entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'");

                        ulong actualSize = (ulong)dataStream.Length;
                        if ((ulong)dataStream.Length != dataHeader.UmcompressedSize)
                            throw new NotSupportedException($"Wrong '{dataHeader.Compression}' decompressed size, expect '{dataHeader.UmcompressedSize}' but got '{actualSize}' for entry {entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'");
                    }

                    Anime anime = null;
                    if (idToAnimeMap.ContainsKey(entry.Aid))
                        anime = idToAnimeMap[entry.Aid];

                    if (entry.Type == DetailsEntryType)
                    {
                        anime = AnimeSerialization.DeserializeAnime(dataStream.ToArray(), entry.Aid);
                        idToAnimeMap[entry.Aid] = anime;
                    }
                    else if (entry.Type == PictureEntryType)
                    {
                        if (anime != null)
                        {
                            // TODO(final): Wrong image filename, dont assume its from anidb always!
                            string filename = anime.Picture;
                            anime.Image = new AnimeImage(filename, dataStream.ToArray());
                        }
                    }

                    dataStream.Dispose();

                    ++entryIndex;
                }
            }

            List<Anime> list = new List<Anime>(idToAnimeMap.Values);

            AnimeStorageData result = new AnimeStorageData(list.ToImmutableArray(), ImmutableArray<Title>.Empty);

            return result;
        }

        public override string ToString() => _filePath;
    }
}
