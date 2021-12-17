using Finalspace.Onigiri.MVVM;

namespace Finalspace.Onigiri.Models
{
    public class Category: BindableBase
    {
        public ulong Id
        {
            get { return GetValue(() => Id); }
            set { SetValue(() => Id, value); }
        }
        public ulong ParentId
        {
            get { return GetValue(() => ParentId); }
            set { SetValue(() => ParentId, value); }
        }
        public string Name
        {
            get { return GetValue(() => Name); }
            set { SetValue(() => Name, value); }
        }
        public string Description
        {
            get { return GetValue(() => Description); }
            set { SetValue(() => Description, value); }
        }
        public int Weight
        {
            get { return GetValue(() => Weight); }
            set { SetValue(() => Weight, value); }
        }

        public override string ToString()
        {
            return $"{Name} ({Weight})";
        }
    }
}
