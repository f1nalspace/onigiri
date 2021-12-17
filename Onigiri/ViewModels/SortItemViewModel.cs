using DevExpress.Mvvm;
using Finalspace.Onigiri.Helper;

namespace Finalspace.Onigiri.ViewModels
{
    public class SortItemViewModel : ViewModelBase
    {
        public string DisplayName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public AnimeSortKey Value
        {
            get => GetValue<AnimeSortKey>();
            set => SetValue(value);
        }
    }
}
