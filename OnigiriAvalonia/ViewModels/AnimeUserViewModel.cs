using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.MVVM;
using System;

namespace Finalspace.Onigiri.ViewModels;

public class AnimeUserViewModel : BindableBase
{
    public Anime Anime { get; }
    public string Username { get; }

    public AnimeUserViewModel(Anime anime, string username)
    {
        ArgumentNullException.ThrowIfNull(anime);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        Anime = anime;
        Username = username;
    }

    public override string ToString() => $"[{Username}] {Anime}";
}
