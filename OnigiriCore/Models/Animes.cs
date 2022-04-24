using log4net;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections;
using System;

namespace Finalspace.Onigiri.Models
{
    public class Animes
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IEnumerable<Anime> Items => _items;
        private ImmutableArray<Anime> _items = ImmutableArray<Anime>.Empty;

        public IEnumerable<AnimeGroup> Groups => _groups;
        private ImmutableArray<AnimeGroup> _groups = ImmutableArray<AnimeGroup>.Empty;

        public Anime FindByAid(ulong aid)
        {
            Anime first = Items.Where((d) => d.Aid == aid).FirstOrDefault();
            if (first != null)
                return first;
            return null;
        }

        public Animes()
        {
        }

        public void Clear()
        {
            _items = ImmutableArray<Anime>.Empty;
            _groups = ImmutableArray<AnimeGroup>.Empty;
        }

        public void Set(params Anime[] animes)
        {
            if (animes == null)
                throw new ArgumentNullException(nameof(animes));

            _items = animes.ToImmutableArray();

            List<AnimeGroup> groups = new List<AnimeGroup>();
            foreach (Anime anime in _items)
            {
                int sequelCount = anime.Relations.Count(c => c.Type == RelationType.Sequel);
                if (sequelCount > 0)
                {
                    AnimeGroup group = new AnimeGroup(anime);
                    Anime sequel = anime;
                    while ((sequel = FindSequel(sequel.Aid)) != null)
                    {
                        group.Items.Add(new AnimeGroupItem(sequel));
                    }
                    groups.Add(group);
                }
            }
            _groups = groups.ToImmutableArray();
        }

        private Anime FindSequel(ulong prequelAid)
        {
            Anime result = Items.FirstOrDefault((a) => a.IsSequal(prequelAid));
            return (result);
        }
    }
}