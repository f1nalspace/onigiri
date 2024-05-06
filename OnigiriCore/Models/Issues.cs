using DevExpress.Mvvm;
using Finalspace.Onigiri.Enums;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Finalspace.Onigiri.Models
{
    public class Issues : BindableBase
    {
        public IReadOnlyCollection<Issue> Items => _items;
        private ImmutableList<Issue> _items = ImmutableList<Issue>.Empty;

        public int Count => Items.Count;

        public Issues()
        {
        }

        public void Add(IssueKind kind, string message, string path, object value = null)
        {
            _items = _items.Add(new Issue(kind, message, path, value));
            RaisePropertyChanged(nameof(Items));
        }

        public void Clear()
        {
            _items = ImmutableList<Issue>.Empty;
            RaisePropertyChanged(nameof(Items));
        }
    }
}
