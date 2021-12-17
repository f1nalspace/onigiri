using DevExpress.Mvvm;
using Finalspace.Onigiri.Helper;
using Finalspace.Onigiri.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Threading;
using System;

namespace Finalspace.Onigiri.ViewModels
{
    public class TitlesViewModel : ViewModelBase, IDisposable
    {
        private BackgroundWorker _filterWorker;
        private Timer _filterTimer;

        public IDispatcherService DispatcherService => GetService<IDispatcherService>();

        public MainViewModel Main
        {
            get => GetValue<MainViewModel>();
            private set => SetValue(value);
        }

        public bool IsNotLoading
        {
            get => GetValue<bool>();
            private set => SetValue(value);
        }

        public Visibility LoadingWindowVisibility => (!IsNotLoading) ? Visibility.Visible : Visibility.Collapsed;

        private void StartedLoading()
        {
            IsNotLoading = false;
            RaisePropertyChanged(() => LoadingWindowVisibility);
        }
        private void FinishedLoading()
        {
            IsNotLoading = true;
            RaisePropertyChanged(() => LoadingWindowVisibility);
        }

        private readonly IList<Title> _titles;
        public ICollectionView TitlesView
        {
            get => GetValue<ICollectionView>();
            private set => SetValue(value);
        }

        public IEnumerable<string> FilterTypes
        {
            get
            {
                List<string> result = new List<string>();
                result.Add("All");
                result.AddRange(_titles.Select(t => t.Type).Distinct());
                return (result);
            }
        }

        private HashSet<ulong> _excludedAnimes;

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
                TitlesView.Refresh();
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

        private bool TitleFilter(object item)
        {
            bool result = true;
            Title title = (Title)item;
            if (result && (!string.IsNullOrEmpty(FilterString)))
            {
                bool allowNameMatch = true;
                if (FilterString.StartsWith("#"))
                {
                    string tmp = FilterString.Substring(1);
                    ulong searchAid = 0;
                    if (ulong.TryParse(tmp, out searchAid))
                    {
                        allowNameMatch = false;
                        if (title.Aid != searchAid)
                            result = false;
                    }
                }
                if (allowNameMatch && !title.Name.ToLower().Contains(FilterString.ToLower()))
                    result = false;
            }
            if (result && _excludedAnimes.Count > 0)
            {
                if (_excludedAnimes.Contains(title.Aid))
                    result = false;
            }
            if (result && (!string.IsNullOrEmpty(SelectedFilterType) && !"All".Equals(SelectedFilterType)))
            {
                if (!title.Type.ToLower().Contains(SelectedFilterType.ToLower()))
                    result = false;
            }
            return (result);
        }

        public void UpdateFilter()
        {
            Debug.Assert(_filterWorker == null || !_filterWorker.IsBusy);
            _filterWorker = new BackgroundWorker();
            _filterWorker.DoWork += _filterWorker_DoWork;
            _filterWorker.RunWorkerCompleted += _filterWorker_RunWorkerCompleted;
            _filterWorker.RunWorkerAsync();
        }

        private void _filterWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FinishedLoading();
        }

        private void _filterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            StartedLoading();
            DispatcherService.Invoke(() => TitlesView.Refresh());
        }

        public void SetTitles(IEnumerable<Title> titles)
        {
            _titles.Clear();
            foreach (Title title in titles)
                _titles.Add(title);
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

        

        private readonly bool _allowButtons;
        public bool AllowButtons => _allowButtons;

        public TitlesViewModel(MainViewModel main, bool allowButtons)
        {
            Main = main;
            _allowButtons = allowButtons;

            _filterTimer = new Timer((c) => UpdateFilter(), null, Timeout.Infinite, Timeout.Infinite);

            _titles = new List<Title>();
            _excludedAnimes = new HashSet<ulong>();
            _selectedFilterType = "All";
            TitlesView = CollectionViewSource.GetDefaultView(_titles);
            TitlesView.Filter = TitleFilter;
            ListCollectionView collView = TitlesView as ListCollectionView;
            collView.CustomSort = new TitleSorter();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _filterTimer.Dispose();
                    _filterWorker?.Dispose();
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
}