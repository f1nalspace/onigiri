using Finalspace.Onigiri.Events;
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

            Anime anime = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Anime));
                using (MemoryStream stream = new MemoryStream(animeData.ToArray()))
                {
                    anime = (Anime)serializer.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed to deserialize anime data from anime '{animeFile.Aid}'!", e);
                return new ExecutionResult<Anime>(e);
            }

            anime.Image = pictureData.Length > 0 ? new AnimeImage(anime.Picture, pictureData) : null;

            return new ExecutionResult<Anime>(anime);
        }

        private static ExecutionResult<AnimeFile> Serialize(Anime anime, string pictureFilePath)
        {
            if (anime.Aid == 0)
                return new ExecutionResult<AnimeFile>(new FormatException($"Anime '{anime}' has in valid aid!"));
            byte[] detailsData = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Anime));
                using (MemoryStream detailsStream = new MemoryStream())
                {
                    serializer.Serialize(detailsStream, anime);
                    detailsStream.Flush();
                    detailsStream.Seek(0, SeekOrigin.Begin);
                    detailsData = detailsStream.ToArray();
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed to serialize anime '{anime}' to xml!", e);
                return new ExecutionResult<AnimeFile>(e);
            }

            byte[] pictureData = null;
            if (!string.IsNullOrEmpty(pictureFilePath))
                pictureData = FileUtils.LoadFileData(pictureFilePath);

            AnimeFile animeFile = new AnimeFile(anime.Aid, detailsData.ToImmutableArray(), pictureData?.ToImmutableArray() ?? ImmutableArray<byte>.Empty);

            return new ExecutionResult<AnimeFile>(animeFile);
        }

        public ImmutableArray<Anime> Load(StatusChangedEventHandler statusChanged)
        {
            ConcurrentBag<Anime> list = new ConcurrentBag<Anime>();

            DirectoryInfo rootDir = new DirectoryInfo(_persistentPath);
            if (rootDir.Exists)
            {
                int count = 0;
                FileInfo[] persistentFiles = rootDir.GetFiles("p*.onigiri");
                int totalFileCount = persistentFiles.Length;
                ParallelOptions poptions = new ParallelOptions() { MaxDegreeOfParallelism = _maxThreadCount };
                Parallel.ForEach(persistentFiles, poptions, (persistentFile) =>
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
                });
            }

            ImmutableArray<Anime> result = list.ToImmutableArray();
            return result;
        }

        public bool Save(ImmutableArray<Anime> animes, StatusChangedEventHandler statusChanged)
        {
            if (animes == null)
                throw new ArgumentNullException(nameof(animes));

            DirectoryInfo rootDir = new DirectoryInfo(_persistentPath);
            if (!rootDir.Exists)
                return false;

            int totalCount = animes.Length;
            int count = 0;
            ParallelOptions poptions = new ParallelOptions() { MaxDegreeOfParallelism = _maxThreadCount };
            Parallel.ForEach(animes, poptions, (anime) =>
            {
                int c = Interlocked.Increment(ref count);
                int percentage = (int)(c / (double)totalCount * 100.0);
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = anime.MainTitle, Percentage = percentage });

                ExecutionResult<AnimeFile> res = Serialize(anime, anime.ImageFilePath);
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
            return true;
        }

        public bool Save(Anime anime, StatusChangedEventHandler statusChanged)
        {
            if (anime == null)
                throw new ArgumentNullException(nameof(anime));

            DirectoryInfo rootDir = new DirectoryInfo(_persistentPath);
            if (!rootDir.Exists)
                return false;

            ExecutionResult<AnimeFile> res = Serialize(anime, anime.ImageFilePath);
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
    }
}