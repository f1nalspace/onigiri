using Finalspace.Onigiri.Models;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Finalspace.Onigiri.Helper;
using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Services;
using Finalspace.Onigiri.MVVM;
using Finalspace.Onigiri.Enums;
using Finalspace.Onigiri.Storage;
using System.Diagnostics;
using Finalspace.Onigiri.Security;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Immutable;
using System.Reflection;
using Serilog;

namespace Finalspace.Onigiri.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private static readonly ILogger log = Log.ForContext<MainViewModel>();

    #region Services
    public OnigiriService CoreService { get; private set; }
    public IOnigiriDialogService DlgService => GetService<IOnigiriDialogService>();
    public IProcessStarterService ProcessStarterService => GetService<IProcessStarterService>();
    public IThemeManagerService ThemeMngService => GetService<IThemeManagerService>();
    #endregion

    #region Events
    public event Action CloseRequested;
    #endregion

    #region Storage
    private readonly IAnimeStorage _currentStorage;
    #endregion

    #region Anime list & properties
    protected readonly List<Anime> _allAnimes = new(4096);

    public ImmutableObservableCollection<Anime> Animes { get; } = new();

    public int VisibleAnimeCount => Animes.Count;

    public int TotalAnimeCount => _allAnimes.Count;
    #endregion

    #region Users
    protected readonly List<User> _allUsers = new(32);

    public ImmutableObservableCollection<User> Users { get; } = new();
    #endregion

    #region Status & Loading
    public bool IsNotLoading
    {
        get => GetValue<bool>();
        private set => SetValue(value);
    }

    public bool IsLoading => !IsNotLoading;

    public string LoadingHeader
    {
        get => GetValue<string>();
        private set => SetValue(value);
    }

    public string LoadingSubject
    {
        get => GetValue<string>();
        private set => SetValue(value);
    }

    public int LoadingPercentage
    {
        get => GetValue<int>();
        private set => SetValue(value, RaiseLoadingProgressChanged);
    }

    public bool IsLoadingMarque => LoadingPercentage <= 0;

    private void RaiseLoadingProgressChanged()
    {
        RaisePropertyChanged(() => IsLoadingMarque);
    }

    private void StartedLoading(string header = "Loading")
    {
        IsNotLoading = false;
        LoadingHeader = header;
        LoadingSubject = string.Empty;
        LoadingPercentage = -1;
        RaisePropertyChanged(() => IsLoading);
    }
    private void FinishedLoading()
    {
        IsNotLoading = true;
        LoadingHeader = "Ready";
        LoadingSubject = string.Empty;
        LoadingPercentage = -1;
        RaisePropertyChanged(() => IsLoading);
    }

    private void ChangedStatus(object sender, StatusChangedArgs e)
    {
        if (e.Header != null)
            LoadingHeader = e.Header;
        if (e.Subject != null)
            LoadingSubject = e.Subject;
        if (e.Percentage > 0 && e.Percentage <= 100)
            LoadingPercentage = e.Percentage;
    }
    #endregion

    #region Anime filtering & sorting helpers

    private bool AnimesViewFilter(Anime anime)
    {
        bool result = true;
        if (result && (!string.IsNullOrEmpty(FilterTitle) && !string.IsNullOrEmpty(anime.MainTitle)))
        {
            bool found = false;
            foreach (Title title in anime.Titles.Items)
            {
                if (anime.MainTitle.Contains(FilterTitle, StringComparison.InvariantCultureIgnoreCase))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                result = false;
        }
        if (result && (FilterType is not null && FilterType != AnyFilterType))
        {
            if (!FilterType.Name.Equals(anime.Type, StringComparison.InvariantCultureIgnoreCase))
                result = false;
        }
        if (result && (FilterWatchState is not null && !string.IsNullOrEmpty(FilterWatchState.FilterProperty)))
        {
            AdditionalData addonData = anime.AddonData;
            Type t = addonData.GetType();
            PropertyInfo prop = t.GetProperty(FilterWatchState.FilterProperty, BindingFlags.Instance | BindingFlags.Public);
            if (prop != null)
            {
                if (!(bool)prop.GetValue(addonData))
                {
                    result = false;
                }
            }
        }
        if (result && (FilterCategory is not null && FilterCategory != AnyFilterCategory))
        {
            Category category = anime.TopCategories.FirstOrDefault((a) => a.Id == FilterCategory.Id);
            if (category != null)
            {
                if (FilterCategoryMinWeight != null)
                {
                    int maxWeight = anime.Categories.Max((a) => a.Weight);
                    int weight = category.Weight;
                    double percentage = weight / (double)maxWeight * 100.0;
                    if (percentage <= FilterCategoryMinWeight.MinWeightPercentage)
                        result = false;
                }
            }
            else
                result = false;
        }
        if (result && FilterMarked)
        {
            AdditionalData addonData = anime.AddonData;
            if (!addonData.Marked)
                result = false;
        }
        if (result && FilterRemove)
        {
            AdditionalData addonData = anime.AddonData;
            if (!(addonData.HasAnniDeleteit || addonData.HasFinalDeleteit))
                result = false;
        }
        return result;
    }

    private IEnumerable<Anime> SortAnimes(IEnumerable<Anime> source)
    {
        AnimeSorter sorter = new AnimeSorter
        {
            FirstSortKey = FirstSortKey?.Value ?? AnimeSortKey.None,
            FirstSortIsDesc = IsFirstSortOrderDesc,
            SecondSortKey = SecondSortKey?.Value ?? AnimeSortKey.None,
            SecondSortIsDesc = IsSecondSortOrderDesc,
        };
        List<Anime> list = source.ToList();
        list.Sort((a, b) => sorter.Compare(a, b));
        return list;
    }

    protected void RefreshAnimes()
    {
        IEnumerable<Anime> filtered = _allAnimes.Where(AnimesViewFilter);
        IEnumerable<Anime> sorted = SortAnimes(filtered);
        Animes.ReplaceAll(sorted);
        RaisePropertyChanged(nameof(VisibleAnimeCount));
        RaisePropertyChanged(nameof(TotalAnimeCount));
    }

    #endregion

    #region Refresh worker
    private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

    class RefreshResult
    {
        public ImmutableArray<CategoryItemViewModel> FilterCategories { get; }
        public ImmutableArray<CategoryItemViewModel> FilterCategoryWeights { get; }

        public RefreshResult(IEnumerable<CategoryItemViewModel> filterCategories, IEnumerable<CategoryItemViewModel> filterCategoryWeights)
        {
            FilterCategories = filterCategories.ToImmutableArray();
            FilterCategoryWeights = filterCategoryWeights.ToImmutableArray();
        }
    }

    private Task<RefreshResult> RefreshAsync() => Task.Run(async () =>
    {
        await CoreService.StartupAsync(new StatusChangedEventHandler(ChangedStatus));

        _allUsers.Clear();
        _allUsers.AddRange(CoreService.Config.Users);

        CoreService.ClearIssues();

        await CoreService.LoadAsync(_currentStorage, new StatusChangedEventHandler(ChangedStatus));

        LoadingPercentage = -1;
        LoadingHeader = "Update listview...";
        LoadingSubject = string.Empty;

        double[] MinWeightPercentages = {
            0.0,
            5.0,
            15.0,
            25.0,
            50.0,
        };

        List<CategoryItemViewModel> filterCatsWeights = new List<CategoryItemViewModel>();
        filterCatsWeights.Add(AnyCategoryWeightItem);

        foreach (double weight in MinWeightPercentages)
        {
            filterCatsWeights.Add(new CategoryItemViewModel()
            {
                DisplayName = $"{weight} %",
                MinWeightPercentage = weight,
            });
        }

        _allAnimes.Clear();
        _allAnimes.AddRange(CoreService.Animes.Items);

        if (_allAnimes.Count == 0)
        {
            _allAnimes.Add(new TestAnimeViewModel());
            _allAnimes.Add(new TestAnimeViewModel());
            _allAnimes.Add(new TestAnimeViewModel());
        }

        List<CategoryItemViewModel> filterCats = new List<CategoryItemViewModel>();

        HashSet<ulong> catIds = new HashSet<ulong>();
        foreach (Anime anime in _allAnimes)
        {
            foreach (Category category in anime.Categories)
            {
                if (!catIds.Contains(category.Id))
                {
                    catIds.Add(category.Id);
                    filterCats.Add(new CategoryItemViewModel()
                    {
                        Id = category.Id,
                        DisplayName = $"{category.Name}",
                    });
                }
            }
        }

        filterCats.Sort((a, b) =>
        {
            return string.Compare(a.DisplayName, b.DisplayName);
        });

        filterCats.Insert(0, AnyFilterCategory);

        RefreshResult result = new RefreshResult(filterCats, filterCatsWeights);

        return result;
    });

    private bool CanRefresh() => _refreshLock.CurrentCount > 0;
    private async void Refresh()
    {
        StartedLoading("Refreshing");

        RefreshResult refreshResult = null;

        await _refreshLock.WaitAsync();
        try
        {
            refreshResult = await RefreshAsync();
        }
        catch (Exception e)
        {
            log.Error(e, "Refresh failed!");
            throw;
        }
        finally
        {
            _refreshLock.Release();
        }

        FilterCategories.Clear();
        FilterCategoryWeights.Clear();

        if (refreshResult is not null)
        {
            FilterCategories.AddRange(refreshResult.FilterCategories);
            FilterCategoryWeights.AddRange(refreshResult.FilterCategoryWeights);
        }

        UpdateSort(false);

        Users.ReplaceAll(_allUsers);

        RefreshAnimes();

        CmdRefresh.RaiseCanExecuteChanged();

        FinishedLoading();
    }
    #endregion

    #region Update worker
    private readonly SemaphoreSlim _updateLock = new SemaphoreSlim(1, 1);

    enum UpdateType
    {
        None = 0,
        Store,
        Database
    }

    private Task UpdateAsync(UpdateType updateType) => Task.Run(async () =>
    {
        bool writeStorage = false;
        UpdateFlags updateFlags = UpdateFlags.None;
        if (updateType == UpdateType.Store)
        {
            updateFlags = UpdateFlags.DownloadTitles | UpdateFlags.DownloadDetails | UpdateFlags.DownloadPicture;
            writeStorage = true;
        }
        else if (updateType == UpdateType.Database)
        {
            updateFlags = UpdateFlags.ReadOnly;
            writeStorage = true;
        }
        else
            throw new NotSupportedException($"The update type '{updateType}' is not supported");

        LoadingHeader = $"Updating {updateType}...";

        CoreService.ClearIssues();

        if (updateFlags != UpdateFlags.None)
            await CoreService.UpdateSourcesAsync(updateFlags, new StatusChangedEventHandler(ChangedStatus));

        if (writeStorage)
            CoreService.Save(_currentStorage, new StatusChangedEventHandler(ChangedStatus));

        LoadingPercentage = -1;

        LoadingHeader = "Update listview...";
        LoadingSubject = string.Empty;
        Anime[] items = CoreService.Animes.Items.ToArray();
        _allAnimes.Clear();
        _allAnimes.AddRange(items);
    });

    private bool CanUpdate(string s) => _updateLock.CurrentCount > 0;
    private async void Update(string updateTypeText)
    {
        if (!Enum.TryParse(updateTypeText, out UpdateType updateType))
            throw new FormatException($"Invalid Update Type '{updateTypeText}'");

        StartedLoading($"Updating {updateType}");

        await _updateLock.WaitAsync();
        try
        {
            await UpdateAsync(updateType);
        }
        catch (Exception e)
        {
            log.Error(e, "Update as '{UpdateType}' failed!", updateType);
        }
        finally
        {
            _updateLock.Release();
        }

        RefreshAnimes();

        CmdRefresh.RaiseCanExecuteChanged();

        FinishedLoading();

        if (CoreService.Issues.Count > 0)
            await ShowIssuesDialogAsync();
    }
    #endregion

    #region Export worker
    private readonly SemaphoreSlim _exportLock = new SemaphoreSlim(1, 1);

    private Task ExportAsync(string filePath) => Task.Run(() =>
    {
        DatabaseAnimeFilesStorage databaseStorage = new DatabaseAnimeFilesStorage(filePath);

        CoreService.Save(databaseStorage, new StatusChangedEventHandler(ChangedStatus));
    });

    private bool CanExport() => _exportLock.CurrentCount > 0;
    private async void Export()
    {
        IFileDialogService dlg = GetService<IFileDialogService>();
        string filePath = await dlg.ShowSaveFileDialogAsync(
            "Save to Onigiri Database",
            "Onigiri database file|*.onigiridb",
            ".onigiridb");

        if (string.IsNullOrEmpty(filePath))
            return;

        StartedLoading("Exporting");

        await _exportLock.WaitAsync();
        try
        {
            await ExportAsync(filePath);
        }
        catch (Exception e)
        {
            log.Error(e, "Export to '{FilePath}' failed!", filePath);
        }
        finally
        {
            _exportLock.Release();
        }

        CmdExport.RaiseCanExecuteChanged();

        FinishedLoading();
    }
    #endregion

    #region Import worker
    private readonly SemaphoreSlim _importLock = new SemaphoreSlim(1, 1);

    private async Task ImportAsync(string filePath)
    {
        DatabaseAnimeFilesStorage databaseStorage = new DatabaseAnimeFilesStorage(filePath);

        await CoreService.LoadAsync(databaseStorage, new StatusChangedEventHandler(ChangedStatus)).ConfigureAwait(false);

        _allAnimes.Clear();
        _allAnimes.AddRange(CoreService.Animes.Items);
    }

    private bool CanImport() => _importLock.CurrentCount > 0;
    private async void Import()
    {
        IFileDialogService dlg = GetService<IFileDialogService>();
        string filePath = await dlg.ShowOpenFileDialogAsync(
            "Open Onigiri Database",
            "Onigiri database file|*.onigiridb");

        if (string.IsNullOrEmpty(filePath))
            return;

        StartedLoading($"Importing from '{filePath}'");

        await _importLock.WaitAsync();
        try
        {
            await ImportAsync(filePath);
        }
        catch (Exception e)
        {
            log.Error(e, "Import from '{FilePath}' failed!", filePath);
        }
        finally
        {
            _importLock.Release();
        }

        RefreshAnimes();

        CmdImport.RaiseCanExecuteChanged();

        FinishedLoading();
    }
    #endregion

    #region Action icons
    private void SaveAddonData(Anime anime)
    {
        string addonFilePath = Path.Combine(anime.FoundPath, OnigiriPaths.AnimeXMLAddonFilename);
        anime.AddonData.SaveToFile(addonFilePath);
    }
    private bool CanAddonDataByChanged(Anime anime)
    {
        return Directory.Exists(anime.FoundPath);
    }

    private bool CanToggleMarked(Anime anime)
        => anime is not null;
    private void ToggleMarked(Anime anime)
    {
        anime.AddonData.Marked = !anime.AddonData.Marked;
        if (CanAddonDataByChanged(anime))
            SaveAddonData(anime);
    }

    private bool CanToggleWatched(AnimeUserViewModel animeUser) => animeUser is not null;
    private void ToggleWatched(AnimeUserViewModel animeUser)
    {
        Anime anime = animeUser.Anime;
        string username = animeUser.Username;
        anime.AddonData.ToggleWatchState(username);
        if (CanAddonDataByChanged(anime))
            SaveAddonData(anime);
    }

    private bool CanToggleDeletion(AnimeUserViewModel animeUser) => animeUser is not null;
    private void ToggleDeletion(AnimeUserViewModel animeUser)
    {
        Anime anime = animeUser.Anime;
        string username = animeUser.Username;
        anime.AddonData.ToggleDeleteit(username);
        if (CanAddonDataByChanged(anime))
            SaveAddonData(anime);
    }
    #endregion

    #region Static collections
    private static readonly SortItemViewModel NotSortedItem = new SortItemViewModel() { DisplayName = "Not sorted", Value = AnimeSortKey.None };

    public IReadOnlyCollection<SortItemViewModel> SortItems => _sortItems;
    private static readonly SortItemViewModel[] _sortItems = new SortItemViewModel[]
    {
        NotSortedItem,
        new SortItemViewModel() { DisplayName = "Sort by title", Value = AnimeSortKey.Title },
        new SortItemViewModel() { DisplayName = "Sort by rating", Value = AnimeSortKey.Rating },
        new SortItemViewModel() { DisplayName = "Sort by start date", Value = AnimeSortKey.StartDate },
        new SortItemViewModel() { DisplayName = "Sort by end date", Value = AnimeSortKey.EndDate },
    };

    private static readonly NameItemViewModel AnyFilterType = new NameItemViewModel() { Name = "Any" };

    public IReadOnlyCollection<NameItemViewModel> AnimeTypes => _animeTypes;
    private static readonly NameItemViewModel[] _animeTypes = new NameItemViewModel[]
    {
        AnyFilterType,
        new NameItemViewModel() { Name = "TV Series" },
        new NameItemViewModel() { Name = "OVA" },
        new NameItemViewModel() { Name = "Movie" },
    };

    private static readonly WatchStateItemViewModel AnyWatchStateItem = new WatchStateItemViewModel() { Name = "Any" };

    public IReadOnlyCollection<NameItemViewModel> WatchStates => _watchStates;
    private static readonly NameItemViewModel[] _watchStates = new NameItemViewModel[]
    {
        AnyWatchStateItem,
        new WatchStateItemViewModel() { Name = "Not watched by Anni", FilterProperty = "HasAnniUnwatched" },
        new WatchStateItemViewModel() { Name = "Not watched by Final", FilterProperty = "HasFinalUnwatched" },
        new WatchStateItemViewModel() { Name = "Not watched by Both", FilterProperty = "HasBothUnwatched" },
    };

    private static readonly CategoryItemViewModel AnyFilterCategory = new CategoryItemViewModel() { DisplayName = "Any" };
    private static readonly CategoryItemViewModel AnyCategoryWeightItem = new CategoryItemViewModel() { DisplayName = "Any" };
    private bool isDisposed;

    public ImmutableObservableCollection<CategoryItemViewModel> FilterCategories { get; } = new();
    public ImmutableObservableCollection<CategoryItemViewModel> FilterCategoryWeights { get; } = new();
    #endregion

    #region Sorting
    public bool ShowSorting
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    public SortItemViewModel FirstSortKey
    {
        get => GetValue<SortItemViewModel>();
        set => SetValue(value, () => UpdateSort(true));
    }
    public SortItemViewModel SecondSortKey
    {
        get => GetValue<SortItemViewModel>();
        set => SetValue(value, () => UpdateSort(true));
    }
    public bool IsFirstSortOrderDesc
    {
        get => GetValue<bool>();
        set => SetValue(value, () => UpdateSort(true));
    }
    public bool IsSecondSortOrderDesc
    {
        get => GetValue<bool>();
        set => SetValue(value, () => UpdateSort(true));
    }
    private void UpdateSort(bool forceRefresh)
    {
        if (forceRefresh)
            RefreshAnimes();
    }

    private void ChangePrimarySortType(SortItemViewModel item)
    {
        FirstSortKey = item;
    }

    private void ChangeSecondarySortType(SortItemViewModel item)
    {
        SecondSortKey = item;
    }
    #endregion

    #region Filters
    public bool ShowFilter
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    public string FilterTitle
    {
        get => GetValue<string>();
        set => SetValue(value, () => UpdateFilter());
    }
    public NameItemViewModel FilterType
    {
        get => GetValue<NameItemViewModel>();
        set => SetValue(value, () => UpdateFilter());
    }
    public WatchStateItemViewModel FilterWatchState
    {
        get => GetValue<WatchStateItemViewModel>();
        set => SetValue(value, () => UpdateFilter());
    }
    public CategoryItemViewModel FilterCategory
    {
        get => GetValue<CategoryItemViewModel>();
        set => SetValue(value, () => UpdateFilter());
    }
    public CategoryItemViewModel FilterCategoryMinWeight
    {
        get => GetValue<CategoryItemViewModel>();
        set => SetValue(value, () => UpdateFilter());
    }
    public bool FilterMarked
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }
    public bool FilterRemove
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    private void ChangeFilterType(NameItemViewModel type)
    {
        FilterType = type;
    }

    private void UpdateFilter()
    {
        RefreshAnimes();
    }
    #endregion

    #region Commands
    public DelegateCommand OnLoadedCommand { get; }

    public DelegateCommand<Anime> CmdToggleMarked { get; }
    public DelegateCommand<AnimeUserViewModel> CmdUserActionToggleDelete { get; }
    public DelegateCommand<AnimeUserViewModel> CmdUserActionToggleWatched { get; }

    public DelegateCommand CmdChangedSort { get; }
    public DelegateCommand<SortItemViewModel> CmdPrimarySortType { get; }
    public DelegateCommand<SortItemViewModel> CmdSecondarySortType { get; }
    public DelegateCommand<NameItemViewModel> CmdChangeFilterType { get; }
    public DelegateCommand CmdChangedFilter { get; }
    public DelegateCommand<MainTheme> CmdChangeTheme { get; }

    public DelegateCommand CmdRefresh { get; }
    public DelegateCommand<string> CmdUpdate { get; }
    public DelegateCommand CmdImport { get; }
    public DelegateCommand CmdExport { get; }
    public DelegateCommand CmdSettings { get; }
    public DelegateCommand CmdExit { get; }

    public DelegateCommand CmdTitles { get; }
    public DelegateCommand CmdIssues { get; }

    public DelegateCommand<Anime> CmdOpenPage { get; }
    public DelegateCommand<Anime> CmdOpenRelations { get; }
    #endregion

    public MainTheme Theme { get => GetValue<MainTheme>(); private set => SetValue(value, () => ChangedTheme(value)); }

    private void SetTheme(MainTheme theme)
    {
        Debug.Assert(theme != MainTheme.Automatic);
        ThemeMngService?.ChangeTheme(theme);
    }

    private void ChangedTheme(MainTheme theme)
    {
        IDarkModeDetectionService service = GetService<IDarkModeDetectionService>();
        if (service != null)
            service.DarkModeChanged -= OnDarkModeChanged;
        if (service != null && theme == MainTheme.Automatic)
        {
            if (service.IsDarkMode)
                SetTheme(MainTheme.Dark);
            else
                SetTheme(MainTheme.Light);
            service.DarkModeChanged += OnDarkModeChanged;
        }
        else if (theme == MainTheme.Dark)
            SetTheme(MainTheme.Dark);
        else
            SetTheme(MainTheme.Light);
    }

    public void OnLoaded()
    {
        Refresh();
    }

    private async void ShowSettingsDialog()
    {
        ConfigViewModel config = new ConfigViewModel(CoreService.Config);
        if (await DlgService.ShowConfigurationDialogAsync(config))
        {
            CoreService.SaveConfig();

            _allUsers.Clear();
            _allUsers.AddRange(config.Config.Users);

            Users.ReplaceAll(_allUsers);
        }
    }

    private async void ShowTitlesDialog()
    {
        using TitlesViewModel titlesViewModel = new TitlesViewModel()
        {
            Main = this,
            AllowButtons = false,
        };

        titlesViewModel.SetTitles(CoreService.Titles.Items);
        titlesViewModel.SetExcludedAnimes(CoreService.Animes.Items.Select((a) => a.Aid));
        titlesViewModel.StartRefreshTimer();
        await DlgService.ShowTitlesDialogAsync(titlesViewModel);
    }

    private async Task ShowIssuesDialogAsync()
    {
        IssuesViewModel issuesViewModel = new IssuesViewModel();
        issuesViewModel.Main = this;
        issuesViewModel.SetIssues(CoreService.Issues.Items);
        await DlgService.ShowIssuesDialogAsync(issuesViewModel);
    }

    private void ShowIssuesDialog()
    {
        _ = ShowIssuesDialogAsync();
    }

    private void OpenPage(Anime anime)
    {
        string url = $"https://anidb.net/perl-bin/animedb.pl?show=anime&aid={anime.Aid}";
        ProcessStarterService.Start(url);
    }

    private void OpenRelations(Anime anime)
    {
        string url = $"https://anidb.net/perl-bin/animedb.pl?show=rel&aid={anime.Aid}";
        ProcessStarterService.Start(url);
    }

    private void OnDarkModeChanged(object sender, bool args)
    {
        if (args)
            SetTheme(MainTheme.Dark);
        else
            SetTheme(MainTheme.Light);
    }

    private void ChangeTheme(MainTheme newTheme)
    {
        Theme = newTheme;
    }

    #region Constructor
    public MainViewModel()
    {
        // Dark mode
        IDarkModeDetectionService darkModeService = GetService<IDarkModeDetectionService>();
        if (darkModeService != null)
        {
            darkModeService.DarkModeChanged += OnDarkModeChanged;
            Theme = MainTheme.Automatic;
            ChangedTheme(Theme);
        }
        else
            Theme = MainTheme.Light;

        // Services
        IUserService userService = OnigiriUserServiceFactory.Instance.Create();
        CoreService = new OnigiriService(userService);

        // Storages
        _currentStorage = new FolderAnimeFilesStorage(OnigiriPaths.PersistentPath, CoreService.Config.MaxThreadCount);

        // Default values
        LoadingHeader = string.Empty;
        LoadingSubject = string.Empty;
        LoadingPercentage = -1;
        IsNotLoading = true;

        FirstSortKey = SortItems.First(s => s.Value == AnimeSortKey.EndDate);
        IsFirstSortOrderDesc = true;

        SecondSortKey = SortItems.First();
        IsFirstSortOrderDesc = false;

        FilterType = AnyFilterType;
        FilterCategory = AnyFilterCategory;
        FilterCategoryMinWeight = AnyCategoryWeightItem;
        FilterWatchState = AnyWatchStateItem;

        // Commands
        OnLoadedCommand = new DelegateCommand(OnLoaded);

        CmdToggleMarked = new DelegateCommand<Anime>(ToggleMarked, CanToggleMarked);
        CmdUserActionToggleWatched = new DelegateCommand<AnimeUserViewModel>(ToggleWatched, CanToggleWatched);
        CmdUserActionToggleDelete = new DelegateCommand<AnimeUserViewModel>(ToggleDeletion, CanToggleDeletion);

        CmdChangedSort = new DelegateCommand(() => UpdateSort(true));
        CmdPrimarySortType = new DelegateCommand<SortItemViewModel>(ChangePrimarySortType);
        CmdSecondarySortType = new DelegateCommand<SortItemViewModel>(ChangeSecondarySortType);
        CmdChangeFilterType = new DelegateCommand<NameItemViewModel>(ChangeFilterType);
        CmdChangedFilter = new DelegateCommand(UpdateFilter);
        CmdChangeTheme = new DelegateCommand<MainTheme>(ChangeTheme);

        CmdExit = new DelegateCommand(() => CloseRequested?.Invoke());
        CmdSettings = new DelegateCommand(ShowSettingsDialog);
        CmdRefresh = new DelegateCommand(Refresh, CanRefresh);
        CmdUpdate = new DelegateCommand<string>(Update, CanUpdate);
        CmdExport = new DelegateCommand(Export, CanExport);
        CmdImport = new DelegateCommand(Import, CanImport);

        CmdOpenPage = new DelegateCommand<Anime>(OpenPage);
        CmdOpenRelations = new DelegateCommand<Anime>(OpenRelations);

        CmdTitles = new DelegateCommand(ShowTitlesDialog);
        CmdIssues = new DelegateCommand(ShowIssuesDialog);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                _updateLock.Dispose();
                _exportLock.Dispose();
                _importLock.Dispose();
                _refreshLock.Dispose();
            }

            isDisposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
