using log4net;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Finalspace.Onigiri.Models
{
    public class Animes
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public List<Anime> Items { get; }

        public List<AnimeGroup> Groups { get; }

        public Anime FindByAid(ulong aid)
        {
            Anime first = Items.Where((d) => d.Aid == aid).FirstOrDefault();
            if (first != null)
                return first;
            return null;
        }

        public Animes()
        {
            Items = new List<Anime>(1024);
            Groups = new List<AnimeGroup>(1024);
        }

        public void Clear()
        {
            Items.Clear();
            Groups.Clear();
        }

        private Anime FindSequel(ulong prequelAid)
        {
            Anime result = Items.FirstOrDefault((a) => a.IsSequal(prequelAid));
            return (result);
        }

        public void RefreshGroups()
        {
            Groups.Clear();
            foreach (Anime anime in Items)
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
                    Groups.Add(group);
                }
            }
        }
    }
}