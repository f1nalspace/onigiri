using Finalspace.Onigiri.Core;
using Finalspace.Onigiri.Models;
using System;
using System.Collections.ObjectModel;
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

namespace Finalspace.Onigiri.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Services       
        public OnigiriService CoreService { get; private set; }
        public IOnigiriDialogService DlgService { get { return GetService<IOnigiriDialogService>(); } }
        public IProcessStarterService ProcessStarterService { get { return GetService<IProcessStarterService>(); } }
        #endregion

        #region Events
        public event Action CloseRequested;
        #endregion

        #region Anime list & properties
        private readonly List<Anime> _animes;

        public ICollectionView AnimesView
        {
            get { return GetValue(() => AnimesView); }
            private set { SetValue(() => AnimesView, value); }
        }

        public int VisibleAnimeCount
        {
            get
            {
                int result = AnimesView.Cast<Anime>().Count();
                return result;
            }
        }

        public int TotalAnimeCount
        {
            get
            {
                int result = _animes.Count;
                return result;
            }
        }
        #endregion

        #region Status & Loading
        public bool IsNotLoading
        {
            get { return GetValue(() => IsNotLoading); }
            internal set { SetValue(() => IsNotLoading, value); }
        }

        public Visibility LoadingWindowVisibility
        {
            get { return (!IsNotLoading) ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string LoadingHeader
        {
            get { return GetValue(() => LoadingHeader); }
            internal set { SetValue(() => LoadingHeader, value); }
        }

        public string LoadingSubject
        {
            get { return GetValue(() => LoadingSubject); }
            internal set { SetValue(() => LoadingSubject, value); }
        }

        public int LoadingPercentage
        {
            get { return GetValue(() => LoadingPercentage); }
            internal set { SetValue(() => LoadingPercentage, value, RaiseLoadingProgressChanged); }
        }

        public bool IsLoadingMarque
        {
            get
            {
                return LoadingPercentage <= 0;
            }
        }

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
            _filterCategoryWeights.Clear();
            foreach (double weight in MinWeightPercentages)
            {
                _filterCategoryWeights.Add(new CategoryItemViewModel()
                {
                    DisplayName = $"{weight} %",
                    MinWeightPercentage = weight,
                });
            }

            _animes.Clear();
            _animes.AddRange(CoreService.Animes.Items);

            _filterCategories.Clear();

            HashSet<ulong> catIds = new HashSet<ulong>();
            foreach (Anime anime in _animes)
            {
                foreach (Category category in anime.Categories)
                {
                    if (!catIds.Contains(category.Id))
                    {
                        catIds.Add(category.Id);
                        _filterCategories.Add(new CategoryItemViewModel()
                        {
                            Id = category.Id,
                            DisplayName = $"{category.Name}",
                        });
                    }
                }
            }

            _filterCategories.Sort((a, b) =>
            {
                return string.Compare(a.DisplayName, b.DisplayName);
            });

            

            UIInvoke(() =>
            {
                FilterCategoryWeights.Clear();
                FilterCategories.Clear();
                FilterCategories.AddRange(_filterCategories);
                FilterCategoryWeights.AddRange(_filterCategoryWeights);
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

            UpdateFlags updateFlags = UpdateFlags.WriteCache;
            if ("Store".Equals(updateType))
                updateFlags = UpdateFlags.DownloadDetails | UpdateFlags.DownloadPicture | UpdateFlags.WriteCache;
            else if ("Titles".Equals(updateType))
                updateFlags = UpdateFlags.DownloadTitles;
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
            CoreService.Cache.Write(anime, pictureFilePath);
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

        private readonly List<CategoryItemViewModel> _filterCategories;
        public ExtendedObservableCollection<CategoryItemViewModel> FilterCategories
        {
            get { return GetValue(() => FilterCategories); }
            private set { SetValue(() => FilterCategories, value); }
        }

        private readonly List<CategoryItemViewModel> _filterCategoryWeights;
        public ExtendedObservableCollection<CategoryItemViewModel> FilterCategoryWeights
        {
            get { return GetValue(() => FilterCategoryWeights); }
            private set { SetValue(() => FilterCategoryWeights, value); }
        }
        #endregion

        #region Sorting
        public SortItemViewModel FirstSortKey
        {
            get { return GetValue(() => FirstSortKey); }
            set { SetValue(() => FirstSortKey, value); }
        }
        public SortItemViewModel SecondSortKey
        {
            get { return GetValue(() => SecondSortKey); }
            set { SetValue(() => SecondSortKey, value); }
        }
        public bool IsFirstSortOrderDesc
        {
            get { return GetValue(() => IsFirstSortOrderDesc); }
            set { SetValue(() => IsFirstSortOrderDesc, value); }
        }
        public bool IsSecondSortOrderDesc
        {
            get { return GetValue(() => IsSecondSortOrderDesc); }
            set { SetValue(() => IsSecondSortOrderDesc, value); }
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
            get { return GetValue(() => FilterTitle); }
            set { SetValue(() => FilterTitle, value); }
        }
        public NameItemViewModel FilterType
        {
            get { return GetValue(() => FilterType); }
            set { SetValue(() => FilterType, value); }
        }
        public WatchStateItemViewModel FilterWatchState
        {
            get { return GetValue(() => FilterWatchState); }
            set { SetValue(() => FilterWatchState, value); }
        }
        public CategoryItemViewModel FilterCategory
        {
            get { return GetValue(() => FilterCategory); }
            set { SetValue(() => FilterCategory, value); }
        }
        public CategoryItemViewModel FilterCategoryMinWeight
        {
            get { return GetValue(() => FilterCategoryMinWeight); }
            set { SetValue(() => FilterCategoryMinWeight, value); }
        }
        public bool FilterMarked
        {
            get { return GetValue(() => FilterMarked); }
            set { SetValue(() => FilterMarked, value); }
        }
        public bool FilterRemove
        {
            get { return GetValue(() => FilterRemove); }
            set { SetValue(() => FilterRemove, value); }
        }

        private bool AnimeFilter(object item)
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
        public BasicDelegateCommand CmdToggleMarked { get; private set; }
        public BasicDelegateCommand CmdToggleWatchedFinal { get; private set; }
        public BasicDelegateCommand CmdToggleWatchedAnni { get; private set; }
        public BasicDelegateCommand CmdToggleDeleteitFinal { get; private set; }
        public BasicDelegateCommand CmdToggleDeleteitAnni { get; private set; }
        public BasicDelegateCommand CmdChangedSort { get; private set; }
        public BasicDelegateCommand CmdChangedFilter { get; private set; }
        public BasicDelegateCommand CmdRefresh { get; private set; }
        public DelegateCommand<string> CmdUpdate { get; private set; }
        public BasicDelegateCommand CmdExit { get; private set; }
        public BasicDelegateCommand CmdTitles { get; private set; }
        public BasicDelegateCommand CmdIssues { get; private set; }
        public BasicDelegateCommand CmdOpenPage { get; private set; }
        public BasicDelegateCommand CmdOpenRelations { get; private set; }
        #endregion

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
            TitlesViewModel titlesViewModel = new TitlesViewModel(this, false);
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

        #region Constructor
        public MainViewModel()
        {
            // Services
            CoreService = new OnigiriService();

            // Collections
            _animes = new List<Anime>(4096);
            AnimesView = new ListCollectionView(_animes) { CustomSort = new AnimeSorter() };
            AnimesView.Filter = AnimeFilter;
            _filterCategories = new List<CategoryItemViewModel>();
            _filterCategoryWeights = new List<CategoryItemViewModel>();
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
            CmdToggleMarked = new BasicDelegateCommand(new Action<object>((a) => ToggleMarked((Anime)a)), (a) => CanAddonDataByChanged((Anime)a));
            CmdToggleWatchedFinal = new BasicDelegateCommand(new Action<object>((a) => ToggleWatched((Anime)a, "final")), (a) => CanAddonDataByChanged((Anime)a));
            CmdToggleWatchedAnni = new BasicDelegateCommand(new Action<object>((a) => ToggleWatched((Anime)a, "anni")), (a) => CanAddonDataByChanged((Anime)a));
            CmdToggleDeleteitFinal = new BasicDelegateCommand(new Action<object>((a) => ToggleDeleteit((Anime)a, "final")), (a) => CanAddonDataByChanged((Anime)a));
            CmdToggleDeleteitAnni = new BasicDelegateCommand(new Action<object>((a) => ToggleDeleteit((Anime)a, "anni")), (a) => CanAddonDataByChanged((Anime)a));
            CmdChangedSort = new BasicDelegateCommand(new Action<object>((o) => UpdateSort(true)));
            CmdChangedFilter = new BasicDelegateCommand(new Action<object>((o) => UpdateFilter()));
            CmdExit = new BasicDelegateCommand(new Action<object>((o) => CloseRequested?.Invoke()));
            CmdRefresh = new BasicDelegateCommand((new Action<object>((o) => RefreshWorker.RunWorkerAsync(false))), (o) => !RefreshWorker.IsBusy);
            CmdUpdate = new DelegateCommand<string>((new Action<string>((o) => UpdateWorker.RunWorkerAsync(o))));
            CmdOpenPage = new BasicDelegateCommand(new Action<object>((a) => OpenPage((Anime)a)));
            CmdOpenRelations = new BasicDelegateCommand(new Action<object>((a) => OpenRelations((Anime)a)));
            CmdTitles = new BasicDelegateCommand((o) => ShowTitlesDialog());
            CmdIssues = new BasicDelegateCommand((o) => ShowIssuesDialog());

            // Refresh directly at startup
            RefreshWorker.RunWorkerAsync();
        }
        #endregion
    }
}
