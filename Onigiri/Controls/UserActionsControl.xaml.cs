using DevExpress.Mvvm;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Finalspace.Onigiri.Controls
{
    /// <summary>
    /// Interaction logic for UserActionsPanel.xaml
    /// </summary>
    public partial class UserActionsPanel : UserControl
    {
        private static readonly List<User> _defaultUsers = new List<User>() {
            new User() { UserName = "final", DisplayName = "Final" },
        };

        public static readonly DependencyProperty ToggleRemoveAnimeCommandProperty =
            DependencyProperty.Register(nameof(ToggleRemoveAnimeCommand), typeof(ICommand), typeof(UserActionsPanel), new PropertyMetadata(defaultValue: null));

        public ICommand ToggleRemoveAnimeCommand
        {
            get => GetValue(ToggleRemoveAnimeCommandProperty) as ICommand;
            set => SetCurrentValue(ToggleRemoveAnimeCommandProperty, value);
        }

        public static readonly DependencyProperty ToggleWatchedAnimeCommandProperty =
            DependencyProperty.Register(nameof(ToggleWatchedAnimeCommand), typeof(ICommand), typeof(UserActionsPanel), new PropertyMetadata(defaultValue: null));

        public ICommand ToggleWatchedAnimeCommand
        {
            get => GetValue(ToggleWatchedAnimeCommandProperty) as ICommand;
            set => SetCurrentValue(ToggleWatchedAnimeCommandProperty, value);
        }

        public static readonly DependencyProperty ToggleMarkedAnimeCommandProperty =
            DependencyProperty.Register(nameof(ToggleMarkedAnimeCommand), typeof(ICommand), typeof(UserActionsPanel), new PropertyMetadata(defaultValue: null));

        public ICommand ToggleMarkedAnimeCommand
        {
            get => GetValue(ToggleMarkedAnimeCommandProperty) as ICommand;
            set => SetCurrentValue(ToggleMarkedAnimeCommandProperty, value);
        }

        public static readonly DependencyProperty UsersViewProperty =
            DependencyProperty.Register(nameof(UsersView), typeof(ICollectionView), typeof(UserActionsPanel), new PropertyMetadata(defaultValue: null));

        public ICollectionView UsersView
        {
            get => GetValue(UsersViewProperty) as ICollectionView;
            set => SetCurrentValue(UsersViewProperty, value);
        }

        public DelegateCommand<AnimeUserViewModel> CmdToggleRemoveAnime { get; }
        public DelegateCommand<AnimeUserViewModel> CmdToggleWatchedAnime { get; }
        public DelegateCommand<Anime> CmdToggleMarkedAnime { get; }

        public UserActionsPanel()
        {
            InitializeComponent();

            CmdToggleRemoveAnime = new DelegateCommand<AnimeUserViewModel>(ToggleRemoveAnime, CanToggleRemoveAnime);
            CmdToggleWatchedAnime = new DelegateCommand<AnimeUserViewModel>(ToggleWatchedAnime, CanToggleWatchedAnime);
            CmdToggleMarkedAnime = new DelegateCommand<Anime>(ToggleMarkedAnime, CanToggleMarkedAnime);
        }

        private bool CanToggleMarkedAnime(Anime anime) => anime is not null;
        private void ToggleMarkedAnime(Anime anime)
        {
            if (ToggleMarkedAnimeCommand?.CanExecute(anime) ?? false)
                ToggleMarkedAnimeCommand.Execute(anime);
        }

        private bool CanToggleRemoveAnime(AnimeUserViewModel animeUser) => animeUser is not null;
        private void ToggleRemoveAnime(AnimeUserViewModel animeUser)
        {
            if (ToggleRemoveAnimeCommand?.CanExecute(animeUser) ?? false)
                ToggleRemoveAnimeCommand.Execute(animeUser);
        }

        private bool CanToggleWatchedAnime(AnimeUserViewModel animeUser) => animeUser is not null;
        private void ToggleWatchedAnime(AnimeUserViewModel animeUser)
        {
            if (ToggleWatchedAnimeCommand?.CanExecute(animeUser) ?? false)
                ToggleWatchedAnimeCommand.Execute(animeUser);
        }

    }
}
