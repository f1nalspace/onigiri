namespace Finalspace.Onigiri.ViewModels
{
    public class WatchStateItemViewModel: NameItemViewModel
    {
        public string FilterProperty
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
    }
}
