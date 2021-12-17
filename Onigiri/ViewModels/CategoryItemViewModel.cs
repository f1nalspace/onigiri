using Finalspace.Onigiri.MVVM;

namespace Finalspace.Onigiri.ViewModels
{
    public class CategoryItemViewModel : ViewModelBase
    {
        public string DisplayName
        {
            get { return GetValue(() => DisplayName); }
            set { SetValue(() => DisplayName, value); }
        }
        public ulong Id
        {
            get { return GetValue(() => Id); }
            set { SetValue(() => Id, value); }
        }
        public double MinWeightPercentage
        {
            get { return GetValue(() => MinWeightPercentage); }
            set { SetValue(() => MinWeightPercentage, value); }
        }
    }
}
