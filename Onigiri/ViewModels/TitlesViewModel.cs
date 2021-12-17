using Finalspace.Onigiri.Helper;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.MVVM;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Data;

namespace Finalspace.Onigiri.ViewModels
{
    public class TitlesViewModel : ViewModelBase
    {
        private BackgroundWorker _filterWorker;
        private System.Threading.Timer _filterTimer;

        public MainViewModel Main
        {
            get { return GetValue(() => Main); }
            private set { SetValue(() => Main, value); }
        }

        public bool IsNotLoading
        {
            get { return GetValue(() => IsNotLoading); }
            internal set { SetValue(() => IsNotLoading, value); }
        }

        public Visibility LoadingWindowVisibility
        {
            get { return (!IsNotLoading) ? Visibility.Visible : Visibility.Collapsed; }
        }

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
            get { return GetValue(() => TitlesView); }
            private set { SetValue(() => TitlesView, value); }
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
            get { return GetValue(() => FilterString); }
            set { SetValue(() => FilterString, value); }
        }
        private string _selectedFilterType;
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
            get { return GetValue(() => SelectedTitle); }
            set
            {
                SetValue(() => SelectedTitle, value, () =>
                {
                    RaisePropertyChanged(() => HasSelectedTitle);
                    RaisePropertyChanged(() => SelectedTitleText);
                });
            }
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
            UIInvoke(() =>
            {
                TitlesView.Refresh();
            });
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
    }
}