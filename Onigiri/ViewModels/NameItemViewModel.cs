using Finalspace.Onigiri.MVVM;

namespace Finalspace.Onigiri.ViewModels
{
    public class NameItemViewModel : ViewModelBase
    {
        public string Name
        {
            get { return GetValue(() => Name); }
            set { SetValue(() => Name, value); }
        }
    }
}
