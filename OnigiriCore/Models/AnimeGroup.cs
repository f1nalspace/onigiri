using Finalspace.Onigiri.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Finalspace.Onigiri.Models
{
    public class AnimeGroup : BindableBase
    {
        public Anime Root
        {
            get { return GetValue(() => Root); }
            private set { SetValue(() => Root, value); }
        }

        public ObservableCollection<AnimeGroupItem> Items
        {
            get { return GetValue(() => Items); }
            private set { SetValue(() => Items, value); }
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
