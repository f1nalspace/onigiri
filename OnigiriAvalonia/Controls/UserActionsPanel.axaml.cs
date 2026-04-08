using Avalonia;
using Avalonia.Controls;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.MVVM;
using Finalspace.Onigiri.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Finalspace.Onigiri.Controls;

public partial class UserActionsPanel : UserControl
{
    public static readonly StyledProperty<ICommand> ToggleRemoveAnimeCommandProperty =
        AvaloniaProperty.Register<UserActionsPanel, ICommand>(nameof(ToggleRemoveAnimeCommand));

    public ICommand ToggleRemoveAnimeCommand
    {
        get => GetValue(ToggleRemoveAnimeCommandProperty);
        set => SetValue(ToggleRemoveAnimeCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> ToggleWatchedAnimeCommandProperty =
        AvaloniaProperty.Register<UserActionsPanel, ICommand>(nameof(ToggleWatchedAnimeCommand));

    public ICommand ToggleWatchedAnimeCommand
    {
        get => GetValue(ToggleWatchedAnimeCommandProperty);
        set => SetValue(ToggleWatchedAnimeCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> ToggleMarkedAnimeCommandProperty =
        AvaloniaProperty.Register<UserActionsPanel, ICommand>(nameof(ToggleMarkedAnimeCommand));

    public ICommand ToggleMarkedAnimeCommand
    {
        get => GetValue(ToggleMarkedAnimeCommandProperty);
        set => SetValue(ToggleMarkedAnimeCommandProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<User>> UsersProperty =
        AvaloniaProperty.Register<UserActionsPanel, ObservableCollection<User>>(nameof(Users));

    public ObservableCollection<User> Users
    {
        get => GetValue(UsersProperty);
        set => SetValue(UsersProperty, value);
    }

    public DelegateCommand<AnimeUserViewModel> CmdToggleRemoveAnime { get; }
    public DelegateCommand<AnimeUserViewModel> CmdToggleWatchedAnime { get; }
    public DelegateCommand<Anime> CmdToggleMarkedAnime { get; }

    public UserActionsPanel()
    {
        CmdToggleRemoveAnime = new DelegateCommand<AnimeUserViewModel>(ToggleRemoveAnime, CanToggleRemoveAnime);
        CmdToggleWatchedAnime = new DelegateCommand<AnimeUserViewModel>(ToggleWatchedAnime, CanToggleWatchedAnime);
        CmdToggleMarkedAnime = new DelegateCommand<Anime>(ToggleMarkedAnime, CanToggleMarkedAnime);
        InitializeComponent();
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
