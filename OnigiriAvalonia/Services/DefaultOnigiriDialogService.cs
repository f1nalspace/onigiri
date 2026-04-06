using Avalonia.Controls;
using Finalspace.Onigiri.ViewModels;
using Finalspace.Onigiri.Views;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Services;

class DefaultOnigiriDialogService : IOnigiriDialogService
{
    private readonly Window _owner;

    public DefaultOnigiriDialogService(Window owner)
    {
        _owner = owner;
    }

    public async Task<bool> ShowTitlesDialogAsync(TitlesViewModel titlesViewModel)
    {
        var window = new TitlesWindow { DataContext = titlesViewModel };
        var result = await window.ShowDialog<bool?>(_owner);
        return result ?? false;
    }

    public async Task ShowIssuesDialogAsync(IssuesViewModel issuesViewModel)
    {
        var window = new IssuesWindow { DataContext = issuesViewModel };
        await window.ShowDialog(_owner);
    }

    public async Task<bool> ShowConfigurationDialogAsync(ConfigViewModel configViewModel)
    {
        var window = new ConfigWindow { DataContext = configViewModel };
        var result = await window.ShowDialog<bool?>(_owner);
        return result ?? false;
    }
}
