using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Models;
using System;
using System.Collections.Generic;

namespace Finalspace.Onigiri.Persistence
{
    public class DatabaseAnimeFilesCache : IAnimeCache
    {
        public IEnumerable<Anime> Animes => throw new NotImplementedException();

        public byte[] GetImageData(ulong aid)
        {
            throw new NotImplementedException();
        }

        public Tuple<ExecutionResult, Anime, byte[]> Deserialize(AnimeFile animeFile)
        {
            throw new NotImplementedException();
        }

        public Tuple<ExecutionResult, AnimeFile> Serialize(Anime anime, string pictureFilePath)
        {
            throw new NotImplementedException();
        }

        public void Load(Config config, StatusChangedEventHandler statusChanged)
        {
            throw new NotImplementedException();
        }
    }
}
