using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Finalspace.Onigiri.Storage
{
    public class DatabaseAnimeFilesCache : IAnimeCache
    {
        public bool Save(Anime anime)
        {
            return false;
        }

        public bool Save(ImmutableArray<Anime> animes, StatusChangedEventHandler statusChanged)
        {
            return false;
        }

        public ImmutableArray<Anime> Load(StatusChangedEventHandler statusChanged)
        {
            throw new NotImplementedException();
        }
    }
}
