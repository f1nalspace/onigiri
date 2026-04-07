using Finalspace.Onigiri.Helper;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.MVVM;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Finalspace.Onigiri.ViewModels;

public class IssuesViewModel : ViewModelBase
{
    public MainViewModel Main
    {
        get => GetValue<MainViewModel>();
        set => SetValue(value);
    }

    private readonly List<Issue> _allIssues = new();
    public ImmutableObservableCollection<Issue> Issues { get; } = new();

    public Issue SelectedIssue
    {
        get => GetValue<Issue>();
        set => SetValue(value);
    }

    public void SetIssues(IEnumerable<Issue> issues)
    {
        _allIssues.Clear();
        foreach (Issue issue in issues)
            _allIssues.Add(issue);

        IssuesSorter sorter = new IssuesSorter();
        List<Issue> sorted = _allIssues.ToList();
        sorted.Sort((a, b) => sorter.Compare(a, b));
        Issues.ReplaceAll(sorted);
    }

    public DelegateCommand<Issue> CmdSelectTitle { get; private set; }

    public IssuesViewModel()
    {
        CmdSelectTitle = new DelegateCommand<Issue>(async (issue) =>
        {
            using TitlesViewModel titlesViewModel = new TitlesViewModel()
            {
                Main = Main,
                AllowButtons = true,
            };

            titlesViewModel.SetTitles(Main.CoreService.Titles.Items);

            titlesViewModel.StartRefreshTimer();

            titlesViewModel.FilterString = string.Empty;

            if (await Main?.DlgService.ShowTitlesDialogAsync(titlesViewModel))
            {
                Title title = titlesViewModel.SelectedTitle;
                if (title != null)
                {
                    Debug.Assert(issue.Path != null);
                    if (!Directory.Exists(issue.Path))
                        throw new DirectoryNotFoundException(issue.Path);
                    string animeAidFilePath = Path.Combine(issue.Path, OnigiriPaths.AnimeAIDFilename);
                    using (StreamWriter writer = new StreamWriter(animeAidFilePath, false, Encoding.UTF8))
                        writer.Write(title.Aid);
                    issue.IsSolved = true;
                }
            }
        }, (issue) => issue.Kind == Enums.IssueKind.TitleNotFound);
    }
}
