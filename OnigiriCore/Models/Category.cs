using Finalspace.Onigiri.MVVM;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    public class Category : BindableBase
    {
        [XmlAttribute("Id")]
        public ulong Id
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        [XmlAttribute("ParentId")]
        public ulong ParentId
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        [XmlElement("Name")]
        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlElement("Description")]
        public string Description
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute("Weight")]
        public int Weight
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        [XmlAttribute("Hentai")]
        public bool Hentai
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public override string ToString() => $"[{Id}, {Weight}] {Name}: {Description}";
    }
}
