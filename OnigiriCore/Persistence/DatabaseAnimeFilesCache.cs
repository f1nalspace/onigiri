using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Finalspace.Onigiri.Persistence
{
    public class DatabaseAnimeFilesCache : IAnimeCache
    {
        public ImmutableArray<Anime> Load(StatusChangedEventHandler statusChanged)
        {
            throw new NotImplementedException();
        }

        public void Save(ImmutableArray<Anime> animes, StatusChangedEventHandler statusChanged)
        {
            throw new NotImplementedException();
        }

        public bool Save(Anime anime)
        {
            throw new NotImplementedException();
        }

        bool IAnimeCache.Save(ImmutableArray<Anime> animes, StatusChangedEventHandler statusChanged)
        {
            throw new NotImplementedException();
        }
    }
}
