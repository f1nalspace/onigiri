using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Finalspace.Onigiri.Helper
{
    [Serializable]
    public class PagingCollectionView : CollectionView
    {
        private readonly IList _innerList;
        private readonly int _itemsPerPage;

        private int _currentPage = 1;

        public PagingCollectionView(IList innerList, int itemsPerPage)
            : base(innerList)
        {
            _innerList = innerList;
            _itemsPerPage = itemsPerPage;
        }

        public override int Count
        {
            get
            {
                if (_innerList.Count == 0) return 0;
                if (_currentPage < PageCount) // page 1..n-1
                {
                    return _itemsPerPage;
                }
                else // page n
                {
                    var itemsLeft = _innerList.Count % _itemsPerPage;
                    if (0 == itemsLeft)
                    {
                        return _itemsPerPage; // exactly itemsPerPage left
                    }
                    else
                    {
                        // return the remaining items
                        return itemsLeft;
                    }
                }
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(new PropertyChangedEventArgs(nameof(CurrentPage))); }
        }

        public int ItemsPerPage { get { return _itemsPerPage; } }

        public int PageCount => (_innerList.Count + _itemsPerPage - 1) / _itemsPerPage;

        private int EndIndex
        {
            get
            {
                var end = _currentPage * _itemsPerPage - 1;
                return (end > _innerList.Count) ? _innerList.Count : end;
            }
        }

        private int StartIndex => (_currentPage - 1) * _itemsPerPage;

        public override object GetItemAt(int index)
        {
            var offset = index % _itemsPerPage;
            return _innerList[StartIndex + offset];
        }

        public void MoveToNextPage()
        {
            if (_currentPage < PageCount)
            {
                CurrentPage += 1;
            }
            Refresh();
        }

        public void MoveToPreviousPage()
        {
            if (_currentPage > 1)
            {
                CurrentPage -= 1;
            }
            Refresh();
        }
    }
}
