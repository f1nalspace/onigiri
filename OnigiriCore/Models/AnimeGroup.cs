using DevExpress.Mvvm;
using System.Collections.ObjectModel;

namespace Finalspace.Onigiri.Models
{
    public class AnimeGroup : BindableBase
    {
        public Anime Root
        {
            get => GetValue<Anime>();
            private set { SetValue(value); }
        }

        public ObservableCollection<AnimeGroupItem> Items
        {
            get => GetValue<ObservableCollection<AnimeGroupItem>>();
            private set => SetValue(value);
        }

        public AnimeGroup(Anime root)
        {

            Root = root;
            Items = new ObservableCollection<AnimeGroupItem>();
            Items.CollectionChanged += (s, e) => RaisePropertyChanged(() => Items);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}
