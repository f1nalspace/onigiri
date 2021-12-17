using DevExpress.Mvvm;

namespace Finalspace.Onigiri.ViewModels
{
    public class CategoryItemViewModel : ViewModelBase
    {
        public string DisplayName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public ulong Id
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }
        public double MinWeightPercentage
        {
            get => GetValue<double>();
            set => SetValue(value);
        }
    }
}
