using Finalspace.Onigiri.ViewModels;
using Finalspace.Onigiri.Views;
using System.Windows;

namespace Finalspace.Onigiri.Services
{
    public class DefaultOnigiriDialogService : IOnigiriDialogService
    {
        private readonly Window _owner;

        public DefaultOnigiriDialogService(Window owner)
        {
            _owner = owner;
        }

        public bool ShowTitlesDialog(TitlesViewModel titlesViewModel)
        {
            TitlesWindow window = new TitlesWindow();
            window.Owner = _owner;
            window.DataContext = titlesViewModel;
            bool? result = window.ShowDialog();
            if (result.HasValue)
                return result.Value;
            else
                return false;
        }

        public void ShowIssuesDialog(IssuesViewModel issuesViewModel)
        {
            IssuesWindow window = new IssuesWindow();
            window.Owner = _owner;
            window.DataContext = issuesViewModel;
            window.ShowDialog();
        }
    }
}
