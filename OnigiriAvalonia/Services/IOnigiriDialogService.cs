using Finalspace.Onigiri.ViewModels;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Services;

public interface IOnigiriDialogService
{
    Task<bool> ShowTitlesDialogAsync(TitlesViewModel titlesViewModel);
    Task ShowIssuesDialogAsync(IssuesViewModel issuesViewModel);
    Task<bool> ShowConfigurationDialogAsync(ConfigViewModel configViewModel);
}
