using Finalspace.Onigiri.Models;
using System;
using System.Collections.Immutable;

namespace Finalspace.Onigiri.Storage
{
    public class AnimeStorageData
    {
        public ImmutableArray<Anime> Animes { get; }
        public ImmutableArray<Title> Titles { get; }

        public AnimeStorageData(ImmutableArray<Anime> animes, ImmutableArray<Title> titles)
        {
            if (animes == null)
                throw new ArgumentNullException(nameof(animes));
            if (titles == null)
                throw new ArgumentNullException(nameof(titles));
            Animes = animes;
            Titles = titles;
        }
    }
}
