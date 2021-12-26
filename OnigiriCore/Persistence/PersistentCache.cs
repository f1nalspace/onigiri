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
    public class PersistentCache
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

        public PersistentCache(int concurrencyLevel, string path)
        {
            _path = path;
            _aidToAnimeMap = new ConcurrentDictionary<ulong, Anime>(concurrencyLevel, 4096);
            _aidToImageMap = new ConcurrentDictionary<ulong, byte[]>(concurrencyLevel, 4096);
        }

        public Tuple<ExecutionResult, Anime, byte[]> Read(PersistentAnime persistentAnime)
        {
            byte[] animeData = persistentAnime.Details;
            if (animeData == null || animeData.Length == 0)
            {
                log.Error($"Failed decode details from anime '{persistentAnime.Aid}'!");
                return null;
            }
            byte[] pictureData = persistentAnime.Picture;

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
                log.Error($"Failed deserialize anime data from anime '{persistentAnime.Aid}'!", e);
                return new Tuple<ExecutionResult, Anime, byte[]>(new ExecutionResult(e), null, null);
            }
            return new Tuple<ExecutionResult, Anime, byte[]>(new ExecutionResult(), anime, pictureData);
        }

        public ExecutionResult Write(Anime anime, string pictureFilePath)
        {
            if (anime.Aid == 0)
                return new ExecutionResult(new FormatException($"Anime '{anime}' has in valid aid!"));
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
                log.Error($"Failed serialize anime '{anime}' to xml!", e);
                return new ExecutionResult(e);
            }

            byte[] pictureData = null;
            if (!string.IsNullOrEmpty(pictureFilePath))
                pictureData = FileUtils.LoadFileData(pictureFilePath);

            string persistentAnimeFilePath = Path.Combine(_path, $"p{anime.Aid}.onigiri");
            PersistentAnime persistentAnime = new PersistentAnime(anime.Aid, detailsData, pictureData);
            persistentAnime.SaveToFile(persistentAnimeFilePath);
            return new ExecutionResult();
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

                    PersistentAnime persistentAnime = PersistentAnime.LoadFromFile(persistentFile.FullName);
                    if (persistentAnime != null)
                    {
                        var t = Read(persistentAnime);
                        if (t.Item1.Success)
                        {
                            _aidToAnimeMap.AddOrUpdate(persistentAnime.Aid, t.Item2, (index, anime) => { return t.Item2; });
                            _aidToImageMap.AddOrUpdate(persistentAnime.Aid, t.Item3, (index, anime) => { return t.Item3; });
                        }
                    }
                });
            }
        }
    }
}