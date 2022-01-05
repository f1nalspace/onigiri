using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Models;
using System;
using System.Collections.Generic;

namespace Finalspace.Onigiri.Persistence
{
    public interface IAnimeCache
    {
        IEnumerable<Anime> Animes { get; }

        byte[] GetImageData(ulong aid);

        void Load(Config config, StatusChangedEventHandler statusChanged);

        Tuple<ExecutionResult, AnimeFile> Serialize(Anime anime, string pictureFilePath);
        Tuple<ExecutionResult, Anime, byte[]> Deserialize(AnimeFile animeFile);
    }
}
