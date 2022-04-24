using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Extensions;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.Types;
using Finalspace.Onigiri.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Serialization;

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
            public uint IsReadonly;
            public uint EntryCount;
            public uint AnimeCount;
            public uint Reserved;

            public ulong TableOffset;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        struct DatabaseFileTableEntry
        {
            public ulong Aid;
            public ulong Offset; // The offset to the DatabaseFileEntryHeader
            public ulong Size; // The compressed data size
            public FourCC Type;
            public FourCC Format;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        struct DatabaseFileEntryHeader
        {
            public static uint MagicKey = 1440374961; // CRC32 hash from DatabaseFileEntryHeader

            public uint Magic; // The magic key for the file entry header
            public uint CRC32; // CRC32 hash from uncompressed data
            public FourCC Compression; // The compression format
            public uint Reserved;

            public DateTime Date; // Stored in UTC
            public ulong UmcompressedSize; // The uncompressed data size
            // ... the compressed byte data
        }

        private static readonly FourCC DetailsType = FourCC.FromString("DTLS");
        private static readonly FourCC PictureType = FourCC.FromString("PICT");

        private static readonly FourCC XMLFormatType = FourCC.FromString("XML_");
        private static readonly FourCC BinaryFormatType = FourCC.FromString("BINY");

        private static readonly FourCC DeflateCompressionType = FourCC.FromString("DEFL");
        private static readonly FourCC GZipCompressionType = FourCC.FromString("GZIP");

        private readonly string _filePath;

        public DatabaseAnimeFilesStorage(string filePath)
        {
            _filePath = filePath;
        }

        public bool Save(Anime anime, StatusChangedEventHandler statusChanged)
        {
            // - Load database file
            // - Load table entries into array
            // - Jump to table offset
            // - Serialize anime and write details and picture (Append a new entry for details and picture)
            // - Write all table entries
            // - Jump to the beginning and update header

            return false;
        }

        public bool Save(ImmutableArray<Anime> animes, StatusChangedEventHandler statusChanged)
        {
            using FileStream fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write);

            // Write empty header first, so we can fill it out later
            fileStream.WriteStruct(new DatabaseFileHeader());

            ulong offset = (ulong)Marshal.SizeOf(typeof(DatabaseFileHeader));

            uint animeCount = (uint)animes.Length;

            List<DatabaseFileTableEntry> entries = new List<DatabaseFileTableEntry>();

            // Write details & picture foreach
            int count = 0;
            int totalCount = animes.Length;
            foreach (Anime anime in animes)
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

                    DatabaseFileEntryHeader dataHeader = new DatabaseFileEntryHeader()
                    {
                        Magic = DatabaseFileEntryHeader.MagicKey,
                        CRC32 = crc,
                        Date = date,
                        UmcompressedSize = uncompressedSize,
                        Compression = GZipCompressionType,
                    };
                    fileStream.WriteStruct(dataHeader);

                    compressedStream.CopyTo(fileStream);

                    DatabaseFileTableEntry entry = new DatabaseFileTableEntry()
                    {
                        Aid = anime.Aid,
                        Offset = offset,
                        Size = size,
                        Format = XMLFormatType,
                        Type = DetailsType,
                    };
                    entries.Add(entry);

                    offset += (ulong)Marshal.SizeOf(typeof(DatabaseFileEntryHeader));
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

                        DatabaseFileEntryHeader dataHeader = new DatabaseFileEntryHeader()
                        {
                            Magic = DatabaseFileEntryHeader.MagicKey,
                            CRC32 = crc,
                            Date = date,
                            Compression = FourCC.Empty,
                            UmcompressedSize = size,
                        };
                        fileStream.WriteStruct(dataHeader);

                        fileStream.Write(image.Data.AsSpan());

                        DatabaseFileTableEntry entry = new DatabaseFileTableEntry()
                        {
                            Aid = anime.Aid,
                            Offset = offset,
                            Size = size,
                            Format = BinaryFormatType,
                            Type = PictureType,
                        };
                        entries.Add(entry);

                        offset += (ulong)Marshal.SizeOf(typeof(DatabaseFileEntryHeader));
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
            foreach (DatabaseFileTableEntry entry in entries)
                fileStream.WriteStruct(entry);
            fileStream.Flush();

            // Overwrite header
            DatabaseFileHeader header = new DatabaseFileHeader()
            {
                Magic = DatabaseFileHeader.MagicKey,
                IsReadonly = 0,
                EntryCount = (uint)entries.Count,
                AnimeCount = animeCount,
                TableOffset = tableOffset,
                Version = (uint)DatabaseFileVersion.Latest,
            };
            fileStream.Seek(0, SeekOrigin.Begin);
            fileStream.WriteStruct(header);
            fileStream.Flush();

            return true;
        }

        public ImmutableArray<Anime> Load(StatusChangedEventHandler statusChanged)
        {
            if (!File.Exists(_filePath))
                return ImmutableArray<Anime>.Empty;

            FileInfo fileInfo = new FileInfo(_filePath);
            long fileLen = fileInfo.Length;

            Dictionary<ulong, Anime> idToAnimeMap = new Dictionary<ulong, Anime>();

            using (FileStream fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                DatabaseFileHeader header = fileStream.ReadStruct<DatabaseFileHeader>();
                if (header.Magic != DatabaseFileHeader.MagicKey ||
                    header.EntryCount == uint.MaxValue ||
                    header.AnimeCount == uint.MaxValue ||
                    header.TableOffset == ulong.MaxValue)
                    throw new FormatException($"Invalid header in database file '{_filePath}'!");

                if (header.Version == 0 || header.Version == uint.MaxValue || header.Version > (uint)DatabaseFileVersion.Latest)
                    throw new FormatException($"Header version of '{header.Version}' is not supported in database file '{_filePath}'!");

                long lastTableOffset = (long)header.TableOffset + header.EntryCount * Marshal.SizeOf(typeof(DatabaseFileTableEntry));
                if (header.TableOffset == 0 || (long)header.TableOffset >= fileLen || lastTableOffset > fileLen)
                    throw new FormatException($"Invalid table offset '{header.TableOffset}' in database file '{_filePath}'!");

                fileStream.Seek((long)header.TableOffset, SeekOrigin.Begin);

                DatabaseFileTableEntry[] entries = new DatabaseFileTableEntry[header.EntryCount];
                for (int i = 0; i < entries.Length; i++)
                    entries[i] = fileStream.ReadStruct<DatabaseFileTableEntry>();

                // TODO(final): Parallel load!
                int entryIndex = 0;
                int entryCount = entries.Length;
                foreach (DatabaseFileTableEntry entry in entries)
                {
                    if (entry.Aid == 0 || entry.Aid >= uint.MaxValue)
                        throw new FormatException($"Invalid aid of '{entry.Aid}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");

                    if (entry.Type == FourCC.Empty)
                        throw new FormatException($"Invalid type of '{entry.Type}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");
                    if (!(entry.Type == DetailsType || entry.Type == PictureType))
                        throw new FormatException($"Unsupported type of '{entry.Type}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");

                    if (entry.Format == FourCC.Empty)
                        throw new FormatException($"Invalid format of '{entry.Format}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");
                    if (!(entry.Format == BinaryFormatType || entry.Format == XMLFormatType))
                        throw new FormatException($"Unsupported format of '{entry.Format}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");

                    if (entry.Size == 0 || entry.Size >= uint.MaxValue)
                        throw new FormatException($"Invalid size of '{entry.Size}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");

                    if (entry.Offset == 0 || entry.Offset >= uint.MaxValue || entry.Offset > ((ulong)fileLen - entry.Size))
                        throw new FormatException($"Invalid offset of '{entry.Offset}' for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");

                    int c = entryIndex + 1;
                    int percentage = (int)(c / (double)entryCount * 100.0);
                    statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = $"Entry {c} of {entryCount}", Percentage = percentage });

                    fileStream.Seek((long)entry.Offset, SeekOrigin.Begin);

                    DatabaseFileEntryHeader dataHeader = fileStream.ReadStruct<DatabaseFileEntryHeader>();
                    if (dataHeader.Magic != DatabaseFileEntryHeader.MagicKey)
                        throw new FormatException($"Invalid magic for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");
                    if (dataHeader.Compression != FourCC.Empty && (dataHeader.UmcompressedSize == 0 || dataHeader.UmcompressedSize == ulong.MaxValue))
                        throw new FormatException($"Invalid uncompression size for entry '{entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");

                    // TODO(final): Read in blocks of int32?
                    if (entry.Size > int.MaxValue)
                        throw new FormatException($"Size of '{entry.Size}' is too large for entry {entryIndex}' with aid '{entry.Aid}' as type '{entry.Type}'!");
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

                    if (entry.Type == DetailsType)
                    {
                        anime = AnimeSerialization.DeserializeAnime(dataStream.ToArray(), entry.Aid);
                        idToAnimeMap[entry.Aid] = anime;
                    }
                    else if (entry.Type == PictureType)
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
            return list.ToImmutableArray();
        }

        public override string ToString() => _filePath;
    }
}
