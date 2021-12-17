using Finalspace.Onigiri.ViewModels;
using System.Windows;

namespace Finalspace.Onigiri.Services
{
    public interface IOnigiriDialogService
    {
        bool ShowTitlesDialog(TitlesViewModel titlesViewModel);
        void ShowIssuesDialog(IssuesViewModel issuesViewModel);
    }
}
