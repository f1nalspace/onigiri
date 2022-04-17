using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.Utils;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Persistence
{
    public class FolderAnimeFilesCache : IAnimeCache
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Config _config;
        private readonly string _path;

        public FolderAnimeFilesCache(Config config, string path)
        {
            _config = config;
            _path = path;
        }

        private static Tuple<ExecutionResult, Anime, ImmutableArray<byte>> Deserialize(AnimeFile animeFile)
        {
            if (animeFile == null)
                throw new ArgumentNullException(nameof(animeFile));
            ImmutableArray<byte> animeData = animeFile.Details;
            if (animeData.Length == 0)
            {
                log.Error($"Failed to decode details from anime '{animeFile.Aid}'!");
                return null;
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
                return new Tuple<ExecutionResult, Anime, ImmutableArray<byte>>(new ExecutionResult(e), null, ImmutableArray<byte>.Empty);
            }
            return new Tuple<ExecutionResult, Anime, ImmutableArray<byte>>(new ExecutionResult(), anime, pictureData);
        }

        private static (ExecutionResult res, AnimeFile file) Serialize(Anime anime, string pictureFilePath)
        {
            if (anime.Aid == 0)
                return (new ExecutionResult(new FormatException($"Anime '{anime}' has in valid aid!")), null);
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
                return (new ExecutionResult(e), null);
            }

            byte[] pictureData = null;
            if (!string.IsNullOrEmpty(pictureFilePath))
                pictureData = FileUtils.LoadFileData(pictureFilePath);

            AnimeFile animeFile = new AnimeFile(anime.Aid, detailsData.ToImmutableArray(), pictureData?.ToImmutableArray() ?? ImmutableArray<byte>.Empty);

            return (new ExecutionResult(), animeFile);
        }

        public ImmutableArray<Anime> Load(StatusChangedEventHandler statusChanged)
        {
            ConcurrentBag<Anime> list = new ConcurrentBag<Anime>();

            DirectoryInfo rootDir = new DirectoryInfo(_path);
            if (rootDir.Exists)
            {
                int count = 0;
                FileInfo[] persistentFiles = rootDir.GetFiles("p*.onigiri");
                int totalFileCount = persistentFiles.Length;
                ParallelOptions poptions = new ParallelOptions() { MaxDegreeOfParallelism = _config.MaxThreadCount };
                Parallel.ForEach(persistentFiles, poptions, (persistentFile) =>
                {
                    int c = Interlocked.Increment(ref count);
                    int percentage = (int)((c / (double)totalFileCount) * 100.0);
                    statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = persistentFile.Name, Percentage = percentage });

                    Tuple<ExecutionResult, AnimeFile> res = AnimeFile.LoadFromFile(persistentFile.FullName);
                    if (res.Item1.Success)
                    {
                        AnimeFile animeFile = res.Item2;
                        Tuple<ExecutionResult, Anime, ImmutableArray<byte>> t = Deserialize(animeFile);
                        if (t.Item1.Success)
                        {
                            Anime anime = t.Item2;
                            list.Add(anime);

                            if (t.Item3.Length > 0)
                                anime.Image = new AnimeImage(anime.ImageFilePath, t.Item3);
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
            int totalCount = animes.Length;
            int count = 0;
            ParallelOptions poptions = new ParallelOptions() { MaxDegreeOfParallelism = _config.MaxThreadCount };
            Parallel.ForEach(animes, poptions, (anime) =>
            {
                int c = Interlocked.Increment(ref count);
                int percentage = (int)((c / (double)totalCount) * 100.0);
                statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = anime.MainTitle, Percentage = percentage });

                (ExecutionResult res, AnimeFile file) res = Serialize(anime, anime.ImageFilePath);
                if (res.res.Success)
                {
                    AnimeFile animeFile = res.file;
                    string persistentAnimeFilePath = Path.Combine(_path, $"p{anime.Aid}.onigiri");
                    animeFile.SaveToFile(persistentAnimeFilePath);
                }
                else
                {
                    // TODO(tspaete): Log issue!
                }
            });
            return true;
        }

        public bool Save(Anime anime)
        {
            if (anime == null)
                throw new ArgumentNullException(nameof(anime));
            (ExecutionResult res, AnimeFile file) res = Serialize(anime, anime.ImageFilePath);
            if (res.res.Success)
            {
                AnimeFile animeFile = res.file;
                string persistentAnimeFilePath = Path.Combine(_path, $"p{anime.Aid}.onigiri");
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