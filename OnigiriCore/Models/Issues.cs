using DevExpress.Mvvm;
using Finalspace.Onigiri.Enums;
using System.Collections.ObjectModel;

namespace Finalspace.Onigiri.Models
{
    public class Issues : BindableBase
    {
        public ObservableCollection<Issue> Items
        {
            get => GetValue<ObservableCollection<Issue>>();
            set => SetValue(value);
        }
        public int Count => Items.Count;

        public Issues()
        {
            Items = new ObservableCollection<Issue>();
            Items.CollectionChanged += (s, e) => RaisePropertyChanged(() => Items);
        }

        public void Add(IssueKind kind, string message, string path, object value = null)
        {
            Items.Add(new Issue(kind, message, path, value));
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}
