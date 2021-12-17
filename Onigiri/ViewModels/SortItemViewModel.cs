using Finalspace.Onigiri.Helper;
using Finalspace.Onigiri.MVVM;

namespace Finalspace.Onigiri.ViewModels
{
    public class SortItemViewModel : ViewModelBase
    {
        public string DisplayName
        {
            get { return GetValue(() => DisplayName); }
            set { SetValue(() => DisplayName, value); }
        }
        public AnimeSortKey Value
        {
            get { return GetValue(() => Value); }
            set { SetValue(() => Value, value); }
        }
    }
}
