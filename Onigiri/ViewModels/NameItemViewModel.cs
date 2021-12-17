using DevExpress.Mvvm;

namespace Finalspace.Onigiri.ViewModels
{
    public class NameItemViewModel : ViewModelBase
    {
        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
    }
}
