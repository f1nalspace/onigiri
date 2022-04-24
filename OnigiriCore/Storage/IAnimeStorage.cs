using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Models;

namespace Finalspace.Onigiri.Storage
{
    public interface IAnimeStorage
    {
        AnimeStorageData Load(StatusChangedEventHandler statusChanged);
        bool Save(AnimeStorageData data, StatusChangedEventHandler statusChanged);
        bool Save(Anime anime, StatusChangedEventHandler statusChanged);
    }
}
