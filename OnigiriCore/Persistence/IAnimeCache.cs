using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Finalspace.Onigiri.Persistence
{
    public interface IAnimeCache
    {
        IEnumerable<Anime> Animes { get; }

        void Load(Config config, StatusChangedEventHandler statusChanged);

        Tuple<ExecutionResult, AnimeFile> Serialize(Anime anime, string pictureFilePath);
        Tuple<ExecutionResult, Anime, ImmutableArray<byte>> Deserialize(AnimeFile animeFile);
    }
}
