using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Extensions;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.Types;
using Finalspace.Onigiri.Utils;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Storage
{
    public class FolderAnimeFilesStorage : IAnimeStorage
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _persistentPath;
        private readonly int _maxThreadCount;

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

        class AnimeFile
        {
            private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            public ulong Aid { get; }

            public ImmutableArray<byte> Details { get; }

            public ImmutableArray<byte> Picture { get; }

            public AnimeFile(ulong aid, ImmutableArray<byte> details, ImmutableArray<byte> picture)
            {
                Aid = aid;
                Details = details;
                Picture = picture;
            }

            public ExecutionResult SaveToStream(Stream stream, string streamName)
            {
                if (stream == null)
                    throw new ArgumentNullException(nameof(stream));
                if (string.IsNullOrWhiteSpace(streamName))
                    throw new ArgumentNullException(nameof(streamName));
                try
                {
                    int sizeOfHeader = Marshal.SizeOf(typeof(AnimeFileHeader));
                    AnimeFileHeader header = new AnimeFileHeader();
                    header.Magic = AnimeFileHeader.MagicKey;
                    header.Version = AnimeFileHeader.CurrentVersion;
                    header.Aid = Aid;
                    header.DetailsLength = (ulong)Details.Length;
                    header.PictureLength = (ulong)Picture.Length;
                    stream.WriteStruct(header);

                    stream.Write(Details.AsSpan());

                    if (Picture.Length > 0)
                        stream.Write(Picture.AsSpan());

                    stream.Flush();
                }
                catch (Exception e)
                {
                    Exception failed = new Exception($"Failed to save anime database '{Aid}' to stream '{streamName}'!", e);
                    log.Error(failed.Message, failed);
                    return new ExecutionResult(failed);
                }
                return new ExecutionResult();
            }

            public ExecutionResult SaveToFile(string filePath)
            {
                try
                {
                    using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        ExecutionResult success = SaveToStream(stream, filePath);
                        if (!success.Success)
                            throw success.Error;
                    }
                }
                catch (Exception e)
                {
                    Exception failed = new Exception($"Failed to save anime database '{Aid}' to file '{filePath}'!", e);
                    log.Error(failed.Message, failed);
                    return new ExecutionResult(failed);
                }
                return new ExecutionResult();
            }

            public static ExecutionResult<AnimeFile> LoadFromStream(Stream stream, string streamName)
            {
                if (stream == null)
                    throw new ArgumentNullException(nameof(stream));
                if (string.IsNullOrWhiteSpace(streamName))
                    throw new ArgumentNullException(nameof(streamName));
                AnimeFile animeFile = null;
                try
                {
                    AnimeFileHeader header = stream.ReadStruct<AnimeFileHeader>();
                    if (header.Magic != AnimeFileHeader.MagicKey)
                        throw new FormatException($"Wrong magic key");
                    if (header.Aid == 0)
                        throw new FormatException($"Wrong aid in anime file stream");
                    if (header.DetailsLength == 0)
                        throw new FormatException($"Missing details block");

                    byte[] detailsData = new byte[header.DetailsLength];
                    stream.Read(detailsData, 0, (int)header.DetailsLength);

                    byte[] pictureData = null;
                    if (header.PictureLength > 0)
                    {
                        pictureData = new byte[header.PictureLength];
                        stream.Read(pictureData, 0, (int)header.PictureLength);
                    }

                    animeFile = new AnimeFile(header.Aid, detailsData.ToImmutableArray(), pictureData?.ToImmutableArray() ?? ImmutableArray<byte>.Empty);
                }
                catch (Exception e)
                {
                    Exception failed = new Exception($"Failed to load anime database from stream '{stream}'!", e);
                    log.Error(failed.Message, failed);
                    return new ExecutionResult<AnimeFile>(failed);

                }
                return new ExecutionResult<AnimeFile>(animeFile);
            }

            public static ExecutionResult<AnimeFile> LoadFromFile(string filePath)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentNullException(nameof(filePath));
                AnimeFile animeFile = null;
                try
                {
                    if (!File.Exists(filePath))
                        throw new FileNotFoundException($"File '{filePath}' was not found", filePath);
                    using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        ExecutionResult<AnimeFile> res = LoadFromStream(stream, filePath);
                        if (!res.Success)
                            throw res.Error;
                        animeFile = res.Value;
                    }
                }
                catch (Exception e)
                {
                    Exception failed = new Exception($"Failed to load anime database file '{filePath}'!", e);
                    log.Error(failed.Message, e);
                    return new ExecutionResult<AnimeFile>(failed);
                }
                return new ExecutionResult<AnimeFile>(animeFile);
            }
        }

        public FolderAnimeFilesStorage(string persistentPath, int? maxThreadCount = null)
        {
            if (string.IsNullOrWhiteSpace(persistentPath))
                throw new ArgumentNullException(nameof(persistentPath));
            _persistentPath = persistentPath;
            _maxThreadCount = maxThreadCount ?? Environment.ProcessorCount;
        }

        private static ExecutionResult<Anime> Deserialize(AnimeFile animeFile)
        {
            if (animeFile == null)
                throw new ArgumentNullException(nameof(animeFile));

            ImmutableArray<byte> animeData = animeFile.Details;
            if (animeData.Length == 0)
            {
                log.Error($"Failed to decode details from anime '{animeFile.Aid}'!");
                return new ExecutionResult<Anime>(new FileNotFoundException("Anime details are empty!"));
            }

            ImmutableArray<byte> pictureData = animeFile.Picture;

            Anime anime = AnimeSerialization.DeserializeAnime(animeData.AsSpan(), animeFile.Aid);
            if (anime == null)
                return new ExecutionResult<Anime>(new FormatException("Anime serializer is broken!"));

            anime.Image = pictureData.Length > 0 ? new AnimeImage(anime.Picture, pictureData) : null;

            return new ExecutionResult<Anime>(anime);
        }

        private static ExecutionResult<AnimeFile> Serialize(Anime anime)
        {
            if (anime == null)
                throw new ArgumentNullException(nameof(anime));
            if (anime.Aid == 0)
                return new ExecutionResult<AnimeFile>(new FormatException($"Anime '{anime}' has in valid aid!"));

            ImmutableArray<byte> detailsData;
            using (MemoryStream stream = AnimeSerialization.SerializeAnime(anime))
                detailsData = stream.ToArray().ToImmutableArray();

            ImmutableArray<byte> pictureData = anime.Image?.Data ?? ImmutableArray<byte>.Empty;

            AnimeFile animeFile = new AnimeFile(anime.Aid, detailsData, pictureData);

            return new ExecutionResult<AnimeFile>(animeFile);
        }

        public AnimeStorageData Load(StatusChangedEventHandler statusChanged)
        {
            ConcurrentBag<Anime> list = new ConcurrentBag<Anime>();

            DirectoryInfo rootDir = new DirectoryInfo(_persistentPath);
            if (rootDir.Exists)
            {
                int count = 0;
                FileInfo[] persistentFiles = rootDir.GetFiles("p*.onigiri");

                int totalFileCount = persistentFiles.Length;

#if !SINGLE_THREAD_PROCESSING
                ParallelOptions poptions = new ParallelOptions() { MaxDegreeOfParallelism = _maxThreadCount };
                Parallel.ForEach(persistentFiles, poptions,
                    (persistentFile) =>
                    {
                        int c = Interlocked.Increment(ref count);
                        int percentage = (int)(c / (double)totalFileCount * 100.0);
                        statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = persistentFile.Name, Percentage = percentage });

                        ExecutionResult<AnimeFile> res = AnimeFile.LoadFromFile(persistentFile.FullName);
                        if (res.Success)
                        {
                            AnimeFile animeFile = res.Value;
                            ExecutionResult<Anime> t = Deserialize(animeFile);
                            if (t.Success)
                            {
                                Anime anime = t.Value;
                                list.Add(anime);
                            }
                            else
                            {
                                // @TODO(final): Handle error!
                            }
                        }
                        else
                        {
                            // @TODO(final): Handle error!
                        }
                    });
#else
                foreach (FileInfo persistentFile in persistentFiles)
                {
                    int c = Interlocked.Increment(ref count);
                    int percentage = (int)(c / (double)totalFileCount * 100.0);
                    statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = persistentFile.Name, Percentage = percentage });

                    ExecutionResult<AnimeFile> res = AnimeFile.LoadFromFile(persistentFile.FullName);
                    if (res.Success)
                    {
                        AnimeFile animeFile = res.Value;
                        ExecutionResult<Anime> t = Deserialize(animeFile);
                        if (t.Success)
                        {
                            Anime anime = t.Value;
                            list.Add(anime);
                        }
                    }
                    else
                    {
                        // TODO(tspaete): Log issue!
                    }
                }
#endif
            }

            AnimeStorageData result = new AnimeStorageData(list.ToImmutableArray(), ImmutableArray<Title>.Empty);

            return result;
        }

        public bool Save(AnimeStorageData data, StatusChangedEventHandler statusChanged)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            DirectoryInfo rootDir = new DirectoryInfo(_persistentPath);
            if (!rootDir.Exists)
                return false;

            int totalCount = data.Animes.Length;
            int count = 0;

#if !SINGLE_THREAD_PROCESSING
            ParallelOptions poptions = new ParallelOptions() { MaxDegreeOfParallelism = _maxThreadCount };
            Parallel.ForEach(data.Animes, poptions, (anime) =>
            {
                int c = Interlocked.Increment(ref count);
                int percentage = (int)(c / (double)totalCount * 100.0);
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = anime.MainTitle, Percentage = percentage });

                ExecutionResult<AnimeFile> res = Serialize(anime);
                if (res.Success)
                {
                    AnimeFile animeFile = res.Value;
                    string persistentAnimeFilePath = Path.Combine(rootDir.FullName, $"p{anime.Aid}.onigiri");
                    animeFile.SaveToFile(persistentAnimeFilePath);
                }
                else
                {
                    // TODO(tspaete): Log issue!
                }
            });
#else
            foreach (Anime anime in data.Animes)
            {
                int c = Interlocked.Increment(ref count);
                int percentage = (int)(c / (double)totalCount * 100.0);
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = anime.MainTitle, Percentage = percentage });

                ExecutionResult<AnimeFile> res = Serialize(anime);
                if (res.Success)
                {
                    AnimeFile animeFile = res.Value;
                    string persistentAnimeFilePath = Path.Combine(rootDir.FullName, $"p{anime.Aid}.onigiri");
                    animeFile.SaveToFile(persistentAnimeFilePath);
                }
                else
                {
                    // TODO(tspaete): Log issue!
                }
            }
#endif
            return true;
        }

        public bool Save(Anime anime, StatusChangedEventHandler statusChanged)
        {
            if (anime == null)
                throw new ArgumentNullException(nameof(anime));

            DirectoryInfo rootDir = new DirectoryInfo(_persistentPath);
            if (!rootDir.Exists)
                return false;

            ExecutionResult<AnimeFile> res = Serialize(anime);
            if (res.Success)
            {
                AnimeFile animeFile = res.Value;
                string persistentAnimeFilePath = Path.Combine(rootDir.FullName, $"p{anime.Aid}.onigiri");
                animeFile.SaveToFile(persistentAnimeFilePath);
                return true;
            }
            else
            {
                // TODO(tspaete): Log issue!
                return false;
            }
        }

        public override string ToString() => "Persistent-Cache";
    }
}