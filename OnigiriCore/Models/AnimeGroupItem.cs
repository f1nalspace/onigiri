using Finalspace.Onigiri.MVVM;

namespace Finalspace.Onigiri.Models
{
    public class AnimeGroupItem : BindableBase
    {
        public Anime Anime
        {
            get { return GetValue(() => Anime); }
            private set { SetValue(() => Anime, value); }
        }

        public AnimeGroupItem(Anime anime)
        {
            Anime = anime;
        }

        public override string ToString()
        {
            return Anime.ToString();
        }
    }
}
