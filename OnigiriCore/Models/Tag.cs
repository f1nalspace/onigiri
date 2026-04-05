using Finalspace.Onigiri.MVVM;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    public class Tag : BindableBase
    {
        [XmlAttribute()]
        public ulong Id
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public ulong ParentId
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        [XmlElement()]
        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlElement()]
        public string Description
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public int Weight
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public bool IsLocalSpoiler
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public bool IsGlobalSpoiler
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public bool Verified
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public string Update
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public override string ToString() => $"[{Id}, {Weight}] {Name}: {Description}";
    }
}
