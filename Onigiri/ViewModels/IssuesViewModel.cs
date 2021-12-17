using DevExpress.Mvvm;
using Finalspace.Onigiri.Core;
using Finalspace.Onigiri.Helper;
using Finalspace.Onigiri.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Data;

namespace Finalspace.Onigiri.ViewModels
{
    public class IssuesViewModel : ViewModelBase
    {
        public MainViewModel Main
        {
            get => GetValue<MainViewModel>();
            private set => SetValue(value);
        }

        private readonly IList<Issue> _issues;
        public ICollectionView IssuesView
        {
            get => GetValue<ICollectionView>();
            private set => SetValue(value);
        }

        public Issue SelectedIssue
        {
            get => GetValue<Issue>();
            set => SetValue(value);
        }

        public void SetIssues(IEnumerable<Issue> issues)
        {
            _issues.Clear();
            foreach (Issue issue in issues)
                _issues.Add(issue);
            IssuesView.Refresh();
        }

        public DelegateCommand<Issue> CmdSelectTitle { get; private set; }

        public IssuesViewModel(MainViewModel main)
        {
            Main = main;
            _issues = new List<Issue>();
            IssuesView = CollectionViewSource.GetDefaultView(_issues);
            ListCollectionView collView = IssuesView as ListCollectionView;
            collView.CustomSort = new IssuesSorter();
            CmdSelectTitle = new DelegateCommand<Issue>((issue) =>
            {
                TitlesViewModel titlesViewModel = new TitlesViewModel(Main, true);
                titlesViewModel.SetTitles(Main.CoreService.Titles.Items);
                titlesViewModel.StartRefreshTimer();
                titlesViewModel.FilterString = string.Empty; //issue.Value as string;
                if (Main.DlgService.ShowTitlesDialog(titlesViewModel))
                {
                    Title title = titlesViewModel.SelectedTitle;
                    if (title != null)
                    {
                        Debug.Assert(issue.Path != null);
                        if (!Directory.Exists(issue.Path))
                            throw new DirectoryNotFoundException(issue.Path);
                        string animeAidFilePath = Path.Combine(issue.Path, OnigiriService.AnimeAIDFilename);
                        using (StreamWriter writer = new StreamWriter(animeAidFilePath, false, Encoding.UTF8))
                            writer.Write(title.Aid);
                        issue.IsSolved = true;
                    }
                }
            }, (issue) => issue.Kind == Enums.IssueKind.TitleNotFound );
        }
    }
}
