﻿using Finalspace.Onigiri.Core;
using Finalspace.Onigiri.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using System.Windows.Data;
using Finalspace.Onigiri.Helper;
using Finalspace.Onigiri.Events;
using Finalspace.Onigiri.Services;
using Finalspace.Onigiri.MVVM;
using Finalspace.Onigiri.Enums;
using DevExpress.Mvvm;

namespace Finalspace.Onigiri.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Services
        public OnigiriService CoreService { get; private set; }
        public IOnigiriDialogService DlgService => GetService<IOnigiriDialogService>();
        public IProcessStarterService ProcessStarterService => GetService<IProcessStarterService>();
        public IDispatcherService DispatcherService => GetService<IDispatcherService>();
        #endregion

        #region Events
        public event Action CloseRequested;
        #endregion

        #region Anime list & properties
        private readonly List<Anime> _animes;

        public ICollectionView AnimesView { get; }

        public int VisibleAnimeCount => AnimesView.Cast<Anime>().Count();

        public int TotalAnimeCount => _animes.Count;
        #endregion

        #region Status & Loading
        public bool IsNotLoading
        {
            get => GetValue<bool>();
            private set => SetValue(value);
        }

        public Visibility LoadingWindowVisibility => (!IsNotLoading) ? Visibility.Visible : Visibility.Collapsed;

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

        private void StartedLoading()
        {
            IsNotLoading = false;
            LoadingHeader = "Loading";
            LoadingSubject = string.Empty;
            LoadingPercentage = -1;
            RaisePropertyChanged(() => LoadingWindowVisibility);
        }
        private void FinishedLoading()
        {
            IsNotLoading = true;
            LoadingHeader = "Ready";
            LoadingSubject = string.Empty;
            LoadingPercentage = -1;
            RaisePropertyChanged(() => LoadingWindowVisibility);
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

        #region Refresh worker
        private BackgroundWorker RefreshWorker { get; set; }
        private void RefreshWorkerComplete(RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                log.Error("Refresh failed!", e.Error);
            AnimesView.Refresh();
            RaisePropertyChanged(() => VisibleAnimeCount);
            RaisePropertyChanged(() => TotalAnimeCount);
            CmdRefresh.RaiseCanExecuteChanged();
            FinishedLoading();
        }
        private void RefreshWorkerProc()
        {
            StartedLoading();

            LoadingHeader = "Refreshing...";
            CoreService.Startup(new StatusChangedEventHandler(ChangedStatus));

            CoreService.ClearIssues();
            CoreService.RefreshAnimes(new StatusChangedEventHandler(ChangedStatus));
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

            List<CategoryItemViewModel> filterCatsWeights = new List<CategoryItemViewModel>(MinWeightPercentages.Length);
            foreach (double weight in MinWeightPercentages)
            {
                filterCatsWeights.Add(new CategoryItemViewModel()
                {
                    DisplayName = $"{weight} %",
                    MinWeightPercentage = weight,
                });
            }

            _animes.Clear();
            _animes.AddRange(CoreService.Animes.Items);

            if (!_animes.Any())
            {
                _animes.Add(new TestAnimeViewModel());
                _animes.Add(new TestAnimeViewModel());
                _animes.Add(new TestAnimeViewModel());
            }

            List<CategoryItemViewModel> filterCats = new List<CategoryItemViewModel>();

            HashSet<ulong> catIds = new HashSet<ulong>();
            foreach (Anime anime in _animes)
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

            DispatcherService.Invoke(() =>
            {
                FilterCategoryWeights.Clear();
                FilterCategories.Clear();
                FilterCategories.AddRange(filterCats);
                FilterCategoryWeights.AddRange(filterCatsWeights);
                UpdateSort(false);
            });
        }
        #endregion

        #region Update worker
        private BackgroundWorker UpdateWorker { get; set; }
        private void UpdateWorkerComplete(RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                log.Error("Refresh failed!", e.Error);
            AnimesView.Refresh();
            RaisePropertyChanged(() => VisibleAnimeCount);
            RaisePropertyChanged(() => TotalAnimeCount);
            CmdRefresh.RaiseCanExecuteChanged();
            FinishedLoading();

            if (CoreService.Issues.Count > 0)
                ShowIssuesDialog();


        }
        private void UpdateWorkerProc(object value)
        {
            string updateType = (string)value;

            StartedLoading();

            UpdateFlags updateFlags;
            if ("Store".Equals(updateType))
                updateFlags = UpdateFlags.DownloadTitles | UpdateFlags.DownloadDetails | UpdateFlags.DownloadPicture | UpdateFlags.WriteCache;
            else if ("Database".Equals(updateType))
                updateFlags = UpdateFlags.WriteCache;
            else
                throw new NotSupportedException($"The update type '{updateType}' is not supported");

            LoadingHeader = $"Updating {updateType}...";

            CoreService.ClearIssues();

            CoreService.UpdateAnimes(updateFlags, new StatusChangedEventHandler(ChangedStatus));

            CoreService.RefreshAnimes(new StatusChangedEventHandler(ChangedStatus));

            LoadingPercentage = -1;

            LoadingHeader = "Update listview...";
            LoadingSubject = string.Empty;
            Anime[] items = CoreService.Animes.Items.ToArray();
            _animes.Clear();
            _animes.AddRange(items);
        }
        #endregion

        #region Action icons
        private void SaveAddonData(Anime anime)
        {
            DirectoryInfo animeDir = new DirectoryInfo(anime.FoundPath);
            string addonFilePath = Path.Combine(anime.FoundPath, OnigiriService.AnimeXMLAddonFilename);
            anime.AddonData.SaveToFile(addonFilePath);
            string pictureFilePath = CoreService.FindImage(animeDir);
            Tuple<ExecutionResult, Persistence.AnimeFile> res = CoreService.Cache.Serialize(anime, pictureFilePath);
        }
        private bool CanAddonDataByChanged(Anime anime)
        {
            // TODO: This is very slow and called very often - on startup only?
            return Directory.Exists(anime.FoundPath);
        }
        private void ToggleMarked(Anime anime)
        {
            anime.AddonData.Marked = !anime.AddonData.Marked;
            SaveAddonData(anime);
        }
        private void ToggleWatched(Anime anime, string who)
        {
            anime.AddonData.ToggleWatchState(who);
            SaveAddonData(anime);
        }
        private void ToggleDeleteit(Anime anime, string who)
        {
            anime.AddonData.ToggleDeleteit(who);
            SaveAddonData(anime);
        }
        #endregion

        #region Static collections
        public IList<SortItemViewModel> SortItems
        {
            get
            {
                return new SortItemViewModel[] {
                    new SortItemViewModel() { DisplayName = "Not sorted", Value = AnimeSortKey.None },
                    new SortItemViewModel() { DisplayName = "Sort by title", Value = AnimeSortKey.Title },
                    new SortItemViewModel() { DisplayName = "Sort by rating", Value = AnimeSortKey.Rating },
                    new SortItemViewModel() { DisplayName = "Sort by start date", Value = AnimeSortKey.StartDate },
                    new SortItemViewModel() { DisplayName = "Sort by end date", Value = AnimeSortKey.EndDate },
                };
            }
        }

        public IList<NameItemViewModel> AnimeTypes
        {
            get
            {
                return new NameItemViewModel[] {
                    new NameItemViewModel() { Name = "All" },
                    new NameItemViewModel() { Name = "TV Series" },
                    new NameItemViewModel() { Name = "OVA" },
                    new NameItemViewModel() { Name = "Movie" },
                };
            }
        }

        public IList<NameItemViewModel> WatchStates
        {
            get
            {
                return new NameItemViewModel[] {
                    new WatchStateItemViewModel() { Name = "All" },
                    new WatchStateItemViewModel() { Name = "Not watched by Anni", FilterProperty = "HasAnniUnwatched" },
                    new WatchStateItemViewModel() { Name = "Not watched by Final", FilterProperty = "HasFinalUnwatched" },
                    new WatchStateItemViewModel() { Name = "Not watched by Both", FilterProperty = "HasBothUnwatched" },
                };
            }
        }

        public ExtendedObservableCollection<CategoryItemViewModel> FilterCategories { get; }
        public ExtendedObservableCollection<CategoryItemViewModel> FilterCategoryWeights { get; }
        #endregion

        #region Sorting
        public SortItemViewModel FirstSortKey
        {
            get => GetValue<SortItemViewModel>();
            set => SetValue(value);
        }
        public SortItemViewModel SecondSortKey
        {
            get => GetValue<SortItemViewModel>();
            set => SetValue(value);
        }
        public bool IsFirstSortOrderDesc
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
        public bool IsSecondSortOrderDesc
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }
        private void UpdateSort(bool forceRefresh)
        {
            ListCollectionView collView = AnimesView as ListCollectionView;
            AnimeSorter sorter = collView.CustomSort as AnimeSorter;
            if (FirstSortKey != null)
            {
                sorter.FirstSortKey = FirstSortKey.Value;
                sorter.FirstSortIsDesc = IsFirstSortOrderDesc;
            }
            if (SecondSortKey != null)
            {
                sorter.SecondSortKey = SecondSortKey.Value;
                sorter.SecondSortIsDesc = IsSecondSortOrderDesc;
            }
            if (forceRefresh)
                collView.Refresh();
        }
        #endregion

        #region Filters
        public string FilterTitle
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public NameItemViewModel FilterType
        {
            get => GetValue<NameItemViewModel>();
            set => SetValue(value);
        }
        public WatchStateItemViewModel FilterWatchState
        {
            get => GetValue<WatchStateItemViewModel>();
            set => SetValue(value);
        }
        public CategoryItemViewModel FilterCategory
        {
            get => GetValue<CategoryItemViewModel>();
            set => SetValue(value);
        }
        public CategoryItemViewModel FilterCategoryMinWeight
        {
            get => GetValue<CategoryItemViewModel>();
            set => SetValue(value);
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

        private bool AnimesViewFilter(object item)
        {
            bool result = true;
            Anime anime = item as Anime;
            if (result && (!string.IsNullOrEmpty(FilterTitle) && !string.IsNullOrEmpty(anime.MainTitle)))
            {
                if (!anime.MainTitle.ToLower().Contains(FilterTitle.ToLower()))
                    result = false;
            }
            if (result && (FilterType != null && (!"All".Equals(FilterType.Name, StringComparison.InvariantCultureIgnoreCase))))
            {
                if (!FilterType.Name.Equals(anime.Type, StringComparison.InvariantCultureIgnoreCase))
                    result = false;
            }
            if (result && (FilterWatchState != null && !string.IsNullOrEmpty(FilterWatchState.FilterProperty)))
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
            if (result && (FilterCategory != null))
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
        private void UpdateFilter()
        {
            AnimesView.Refresh();
            RaisePropertyChanged(() => VisibleAnimeCount);
        }
        #endregion

        #region Commands
        public DelegateCommand<Anime> CmdToggleMarked { get; }
        public DelegateCommand<Anime> CmdToggleWatchedFinal { get; }
        public DelegateCommand<Anime> CmdToggleWatchedAnni { get; }
        public DelegateCommand<Anime> CmdToggleDeleteitFinal { get; }
        public DelegateCommand<Anime> CmdToggleDeleteitAnni { get; }
        public DelegateCommand CmdChangedSort { get; }
        public DelegateCommand CmdChangedFilter { get; }
        public DelegateCommand CmdRefresh { get; }
        public DelegateCommand<string> CmdUpdate { get; }
        public DelegateCommand CmdExit { get; }
        public DelegateCommand CmdTitles { get; }
        public DelegateCommand CmdIssues { get; }
        public DelegateCommand<Anime> CmdOpenPage { get; }
        public DelegateCommand<Anime> CmdOpenRelations { get; }
        public DelegateCommand<MainTheme> CmdChangeTheme { get; }
        #endregion

        public bool IsDarkMode { get => GetValue<bool>(); private set => SetValue(value); }

        public MainTheme Theme { get => GetValue<MainTheme>(); private set => SetValue(value, () => ChangedTheme(value)); }

        private void ChangedTheme(MainTheme theme)
        {
            IDarkModeDetectionService service = GetService<IDarkModeDetectionService>();
            service.DarkModeChanged -= OnDarkModeChanged;

            if (theme == MainTheme.Automatic)
            {
                IsDarkMode = service.IsDarkMode;
                service.DarkModeChanged += OnDarkModeChanged;
            }
            else if (theme == MainTheme.Dark)
                IsDarkMode = true;
            else
                IsDarkMode = false;
        }

        public void WindowLoaded()
        {
#if false
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Render);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 250);
            timer.Tick += (s, e) => {
                if (CmdRefresh.CanExecute(null))
                    CmdRefresh.Execute(null);
                timer.Stop();
            };
            timer.Start();
#endif
        }

        private void ShowTitlesDialog()
        {
            using TitlesViewModel titlesViewModel = new TitlesViewModel(this, false);
            titlesViewModel.SetTitles(CoreService.Titles.Items);
            titlesViewModel.SetExcludedAnimes(CoreService.Animes.Items.Select((a) => a.Aid));
            titlesViewModel.StartRefreshTimer();
            DlgService.ShowTitlesDialog(titlesViewModel);
        }

        private void ShowIssuesDialog()
        {
            IssuesViewModel issuesViewModel = new IssuesViewModel(this);
            issuesViewModel.SetIssues(CoreService.Issues.Items);
            DlgService.ShowIssuesDialog(issuesViewModel);
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

        private void OnDarkModeChanged(object sender, bool e)
        {
            IsDarkMode = e;
        }

        private void ChangeTheme(MainTheme newTheme)
        {
            Theme = newTheme;
        }

        #region Constructor
        public MainViewModel()
        {
            IDarkModeDetectionService darkModeService = GetService<IDarkModeDetectionService>();

            // Dark mode
            darkModeService.DarkModeChanged += OnDarkModeChanged;
            Theme = MainTheme.Automatic;
            ChangedTheme(Theme);

            // Services
            CoreService = new OnigiriService();

            // Collections
            _animes = new List<Anime>(4096);
            AnimesView = new ListCollectionView(_animes) { CustomSort = new AnimeSorter() };
            AnimesView.Filter = AnimesViewFilter;
            FilterCategories = new ExtendedObservableCollection<CategoryItemViewModel>();
            FilterCategoryWeights = new ExtendedObservableCollection<CategoryItemViewModel>();

            // Workers
            RefreshWorker = new BackgroundWorker();
            RefreshWorker.RunWorkerCompleted += (s, e) => RefreshWorkerComplete(e);
            RefreshWorker.DoWork += (s, e) => RefreshWorkerProc();
            UpdateWorker = new BackgroundWorker();
            UpdateWorker.RunWorkerCompleted += (s, e) => UpdateWorkerComplete(e);
            UpdateWorker.DoWork += (s, e) => UpdateWorkerProc(e.Argument);

            // Default values
            LoadingHeader = "Ready";
            LoadingSubject = string.Empty;
            LoadingPercentage = -1;
            FirstSortKey = SortItems[4];
            IsFirstSortOrderDesc = true;
            SecondSortKey = SortItems[0];
            IsFirstSortOrderDesc = false;
            FilterType = AnimeTypes[0];

            // Commands
            CmdToggleMarked = new DelegateCommand<Anime>(ToggleMarked, CanAddonDataByChanged);
            CmdToggleWatchedFinal = new DelegateCommand<Anime>((a) => ToggleWatched(a, "final"), (a) => CanAddonDataByChanged(a));
            CmdToggleWatchedAnni = new DelegateCommand<Anime>((a) => ToggleWatched(a, "anni"), (a) => CanAddonDataByChanged(a));
            CmdToggleDeleteitFinal = new DelegateCommand<Anime>((a) => ToggleDeleteit(a, "final"), (a) => CanAddonDataByChanged(a));
            CmdToggleDeleteitAnni = new DelegateCommand<Anime>((a) => ToggleDeleteit(a, "anni"), (a) => CanAddonDataByChanged(a));
            CmdChangedSort = new DelegateCommand(() => UpdateSort(true));
            CmdChangedFilter = new DelegateCommand(UpdateFilter);
            CmdExit = new DelegateCommand(() => CloseRequested?.Invoke());
            CmdRefresh = new DelegateCommand(() => RefreshWorker.RunWorkerAsync(false), () => !RefreshWorker.IsBusy);
            CmdUpdate = new DelegateCommand<string>((s) => UpdateWorker.RunWorkerAsync(s));
            CmdOpenPage = new DelegateCommand<Anime>(OpenPage);
            CmdOpenRelations = new DelegateCommand<Anime>(OpenRelations);
            CmdTitles = new DelegateCommand(ShowTitlesDialog);
            CmdIssues = new DelegateCommand(ShowIssuesDialog);
            CmdChangeTheme = new DelegateCommand<MainTheme>(ChangeTheme);

            // Refresh directly at startup
            RefreshWorker.RunWorkerAsync();
        }
        #endregion
    }
}
