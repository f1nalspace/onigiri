using DevExpress.Mvvm;

namespace Finalspace.Onigiri.Models
{
    public class Category : BindableBase
    {
        public ulong Id
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }
        public ulong ParentId
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }
        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public string Description
        {
            get => GetValue<string>();
            set => SetValue(value);
        }
        public int Weight
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        public override string ToString()
        {
            return $"{Name} ({Weight})";
        }
    }
}
