using DevExpress.Mvvm;
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
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public double Value
        {
            get => GetValue<double>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public ulong Count
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }
    }
}
