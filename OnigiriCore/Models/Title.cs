using DevExpress.Mvvm;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    public class Title: BindableBase
    {
        [XmlAttribute()]
        public ulong Aid
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public string Name
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public string Type
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public string Lang
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public override string ToString()
        {
            return $"{Name} (aid: {Aid}, lang: {Lang}, type: {Type})";
        }
    }
}
