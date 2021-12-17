using DevExpress.Mvvm;

namespace Finalspace.Onigiri.Models
{
    public class AnimeGroupItem : BindableBase
    {
        public Anime Anime
        {
            get => GetValue<Anime>();
            private set => SetValue(value);
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
