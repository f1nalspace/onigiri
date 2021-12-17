using Finalspace.Onigiri.MVVM;
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
            get { return GetValue(() => Aid); }
            set { SetValue(() => Aid, value); }
        }

        [XmlAttribute()]
        public string Name
        {
            get { return GetValue(() => Name); }
            set { SetValue(() => Name, value); }
        }

        [XmlAttribute()]
        public string Type
        {
            get { return GetValue(() => Type); }
            set { SetValue(() => Type, value); }
        }

        [XmlAttribute()]
        public string Lang
        {
            get { return GetValue(() => Lang); }
            set { SetValue(() => Lang, value); }
        }

        public override string ToString()
        {
            return $"{Name} (aid: {Aid}, lang: {Lang}, type: {Type})";
        }
    }
}
