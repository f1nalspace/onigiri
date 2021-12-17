using Finalspace.Onigiri.MVVM;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot()]
    public class Rating: BindableBase
    {
        [XmlAttribute()]
        public string Name
        {
            get { return GetValue(() => Name); }
            set { SetValue(() => Name, value); }
        }

        [XmlAttribute()]
        public double Value
        {
            get { return GetValue(() => Value); }
            set { SetValue(() => Value, value); }
        }

        [XmlAttribute()]
        public ulong Count
        {
            get { return GetValue(() => Count); }
            set { SetValue(() => Count, value); }
        }
    }
}
