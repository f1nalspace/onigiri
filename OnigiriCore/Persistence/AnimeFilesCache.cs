using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.Utils;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Persistence
{
    public class AnimeFilesCache : IAnimeDatabase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _path;
        private readonly ConcurrentDictionary<ulong, Anime> _aidToAnimeMap;
        private readonly ConcurrentDictionary<ulong, byte[]> _aidToImageMap;

        public IEnumerable<Anime> Animes
        {
            get
            {
                return _aidToAnimeMap.Values;
            }
        }

        public byte[] GetImageData(ulong aid)
        {
            if (_aidToImageMap.ContainsKey(aid))
            {
                return _aidToImageMap[aid];
            }
            return null;
        }

        public AnimeFilesCache(int concurrencyLevel, string path)
        {
            _path = path;
            _aidToAnimeMap = new ConcurrentDictionary<ulong, Anime>(concurrencyLevel, 4096);
            _aidToImageMap = new ConcurrentDictionary<ulong, byte[]>(concurrencyLevel, 4096);
        }

        public Tuple<ExecutionResult, Anime, byte[]> Deserialize(AnimeFile animeFile)
        {
            if (animeFile == null)
                throw new ArgumentNullException(nameof(animeFile));
            byte[] animeData = animeFile.Details;
            if (animeData == null || animeData.Length == 0)
            {
                log.Error($"Failed to decode details from anime '{animeFile.Aid}'!");
                return null;
            }
            byte[] pictureData = animeFile.Picture;

            Anime anime = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Anime));
                using (MemoryStream stream = new MemoryStream(animeData))
                {
                    anime = (Anime)serializer.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed to deserialize anime data from anime '{animeFile.Aid}'!", e);
                return new Tuple<ExecutionResult, Anime, byte[]>(new ExecutionResult(e), null, null);
            }
            return new Tuple<ExecutionResult, Anime, byte[]>(new ExecutionResult(), anime, pictureData);
        }

        public Tuple<ExecutionResult, AnimeFile> Serialize(Anime anime, string pictureFilePath)
        {
            if (anime.Aid == 0)
                return new Tuple<ExecutionResult, AnimeFile>(new ExecutionResult(new FormatException($"Anime '{anime}' has in valid aid!")), null);
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
                return new Tuple<ExecutionResult, AnimeFile>(new ExecutionResult(e), null);
            }

            byte[] pictureData = null;
            if (!string.IsNullOrEmpty(pictureFilePath))
                pictureData = FileUtils.LoadFileData(pictureFilePath);

            AnimeFile animeFile = new AnimeFile(anime.Aid, detailsData, pictureData);

            string persistentAnimeFilePath = Path.Combine(_path, $"p{anime.Aid}.onigiri");
            animeFile.SaveToFile(persistentAnimeFilePath);

            return new Tuple<ExecutionResult, AnimeFile>(new ExecutionResult(), animeFile);
        }

        public void Load(Config config, StatusChangedEventHandler statusChanged)
        {
            _aidToAnimeMap.Clear();
            _aidToImageMap.Clear();

            DirectoryInfo rootDir = new DirectoryInfo(_path);
            if (rootDir.Exists)
            {
                int count = 0;
                FileInfo[] persistentFiles = rootDir.GetFiles("p*.onigiri");
                int totalFileCount = persistentFiles.Length;
                ParallelOptions poptions = new ParallelOptions() { MaxDegreeOfParallelism = config.MaxThreadCount };
                Parallel.ForEach(persistentFiles, poptions, (persistentFile) =>
                {
                    int c = Interlocked.Increment(ref count);
                    int percentage = (int)((c / (double)totalFileCount) * 100.0);
                    statusChanged?.Invoke(this, new StatusChangedArgs() { Subject = persistentFile.Name, Percentage = percentage });

                    Tuple<ExecutionResult, AnimeFile> res = AnimeFile.LoadFromFile(persistentFile.FullName);
                    if (res.Item1.Success)
                    {
                        AnimeFile animeFile = res.Item2;
                        Tuple<ExecutionResult, Anime, byte[]> t = Deserialize(animeFile);
                        if (t.Item1.Success)
                        {
                            _aidToAnimeMap.AddOrUpdate(animeFile.Aid, t.Item2, (index, anime) => { return t.Item2; });
                            _aidToImageMap.AddOrUpdate(animeFile.Aid, t.Item3, (index, anime) => { return t.Item3; });
                        }
                    }
                    else
                    {
                        // TODO(tspaete): Log issue!
                    }
                });
            }
        }
    }
}