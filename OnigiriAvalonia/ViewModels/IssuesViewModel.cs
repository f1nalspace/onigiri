using Finalspace.Onigiri.Helper;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.MVVM;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Finalspace.Onigiri.ViewModels;

public class IssuesViewModel : ViewModelBase
{
    private readonly List<Issue> _allIssues = new();
    private readonly IssuesSorter _sorter = new();

    public ObservableCollection<Issue> Issues { get; } = new();

    public MainViewModel Main
    {
        get => GetValue<MainViewModel>();
        set => SetValue(value);
    }

    public Issue SelectedIssue
    {
        get => GetValue<Issue>();
        set => SetValue(value);
    }

    public DelegateCommand<Issue> CmdSelectTitle { get; private set; }

    public void SetIssues(IEnumerable<Issue> issues)
    {
        _allIssues.Clear();
        _allIssues.AddRange(issues);
        RefreshIssues();
    }

    private void RefreshIssues()
    {
        var sorted = _allIssues.ToList();
        sorted.Sort((a, b) => _sorter.Compare(a, b));

        Issues.Clear();
        foreach (var issue in sorted)
            Issues.Add(issue);
    }

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

            if (await (Main?.DlgService.ShowTitlesDialogAsync(titlesViewModel) ?? System.Threading.Tasks.Task.FromResult(false)))
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
