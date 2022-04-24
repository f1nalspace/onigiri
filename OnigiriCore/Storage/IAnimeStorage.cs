﻿using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Models;
using System.Collections.Immutable;

namespace Finalspace.Onigiri.Storage
{ 
    public interface IAnimeStorage
    {
        ImmutableArray<Anime> Load(StatusChangedEventHandler statusChanged);
        bool Save(ImmutableArray<Anime> animes, StatusChangedEventHandler statusChanged);
        bool Save(Anime anime, StatusChangedEventHandler statusChanged);
    }
}
