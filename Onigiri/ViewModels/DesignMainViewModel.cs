﻿using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.MVVM;
using System.Collections.ObjectModel;

namespace Finalspace.Onigiri.ViewModels
{
    public class DesignMainViewModel : ViewModelBase
    {
        public ObservableCollection<Anime> AnimesView { get; }
        public bool HasItems => true;

        public DesignMainViewModel()
        {
            AnimesView = new ObservableCollection<Anime>();
            AnimesView.CollectionChanged += (s, e) => RaisePropertyChanged(() => AnimesView);
        }
    }
}
