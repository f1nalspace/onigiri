using Finalspace.Onigiri.Helper;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.MVVM;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Finalspace.Onigiri.ViewModels;

public class TitlesViewModel : ViewModelBase, IDisposable
{
    private Timer _filterTimer;

    public MainViewModel Main
    {
        get => GetValue<MainViewModel>();
        set => SetValue(value);
    }

    public bool IsNotLoading
    {
        get => GetValue<bool>();
        private set => SetValue(value);
    }

    public bool IsLoading => !IsNotLoading;

    private void StartedLoading()
    {
        IsNotLoading = false;
        RaisePropertyChanged(() => IsLoading);
    }
    private void FinishedLoading()
    {
        IsNotLoading = true;
        RaisePropertyChanged(() => IsLoading);
    }

    private readonly List<Title> _allTitles = new();

    public ImmutableObservableCollection<Title> FilteredTitles { get; } = new();

    public IEnumerable<string> FilterTypes
    {
        get
        {
            List<string> result = new List<string>();
            result.Add("All");
            result.AddRange(_allTitles.Select(t => t.Type).Distinct());
            return result;
        }
    }

    private readonly HashSet<ulong> _excludedAnimes;

    public string FilterString
    {
        get => GetValue<string>();
        set => SetValue(value);
    }
    private string _selectedFilterType;
    private bool _isDisposed;

    public string SelectedFilterType
    {
        get { return _selectedFilterType; }
        set
        {
            _selectedFilterType = value;
            RaisePropertyChanged(() => SelectedFilterType);
            RefreshTitles();
        }
    }

    public Title SelectedTitle
    {
        get => GetValue<Title>();
        set => SetValue(value, () => RaisePropertiesChanged(nameof(HasSelectedTitle), nameof(SelectedTitleText)));
    }

    public bool HasSelectedTitle => SelectedTitle != null;

    public string SelectedTitleText
    {
        get
        {
            if (SelectedTitle != null)
                return $"{SelectedTitle.Aid} / {SelectedTitle.Name} / {SelectedTitle.Type} / {SelectedTitle.Lang}";
            else
                return null;
        }
    }

    private bool TitleFilter(Title title)
    {
        bool result = true;
        if (result && (!string.IsNullOrEmpty(FilterString)))
        {
            bool allowNameMatch = true;
            if (FilterString.StartsWith("#"))
            {
                string tmp = FilterString.Substring(1);
                if (ulong.TryParse(tmp, out ulong searchAid))
                {
                    allowNameMatch = false;
                    if (title.Aid != searchAid)
                        result = false;
                }
            }
            if (allowNameMatch && !title.Name.Contains(FilterString, StringComparison.InvariantCultureIgnoreCase))
                result = false;
        }
        if (result && _excludedAnimes.Count > 0)
        {
            if (_excludedAnimes.Contains(title.Aid))
                result = false;
        }
        if (result && (!string.IsNullOrEmpty(SelectedFilterType) && !"All".Equals(SelectedFilterType)))
        {
            if (!title.Type.Contains(SelectedFilterType, StringComparison.InvariantCultureIgnoreCase))
                result = false;
        }
        return result;
    }

    private void RefreshTitles()
    {
        IEnumerable<Title> filtered = _allTitles.Where(TitleFilter);
        TitleSorter sorter = new TitleSorter();
        List<Title> sorted = filtered.ToList();
        sorted.Sort((a, b) => sorter.Compare(a, b));
        FilteredTitles.ReplaceAll(sorted);
    }

    public void UpdateFilter()
    {
        StartedLoading();
        Task.Run(() =>
        {
            RefreshTitles();
            FinishedLoading();
        });
    }

    public void SetTitles(IEnumerable<Title> titles)
    {
        _allTitles.Clear();
        foreach (Title title in titles)
            _allTitles.Add(title);
    }

    public void SetExcludedAnimes(IEnumerable<ulong> aidList)
    {
        _excludedAnimes.Clear();
        foreach (ulong aid in aidList)
            _excludedAnimes.Add(aid);
    }

    public void StartRefreshTimer()
    {
        _filterTimer.Change(250, Timeout.Infinite);
    }

    public bool AllowButtons
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    public TitlesViewModel()
    {
        Main = null;
        AllowButtons = true;
        IsNotLoading = true;

        _filterTimer = new Timer((c) => UpdateFilter(), null, Timeout.Infinite, Timeout.Infinite);

        _allTitles.AddRange(new[] {
            new Title() { Name = "blubb", Aid = 42, Type = "main", Lang = "en" },
        });

        _excludedAnimes = new HashSet<ulong>();

        _selectedFilterType = "All";
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _filterTimer.Dispose();
            }
            _isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
