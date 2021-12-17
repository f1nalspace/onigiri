using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot()]
    public class Episode: BindableBase
    {
        [XmlAttribute()]
        public ulong Id
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public string Num
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public ulong NumType
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        [XmlAttribute()]
        public ulong Length
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        [XmlElement()]
        public DateTime? AirDate
        {
            get => GetValue<DateTime?>();
            set => SetValue(value);
        }

        [XmlElement()]
        public DateTime? UpDate
        {
            get => GetValue<DateTime?>();
            set => SetValue(value);
        }

        [XmlElement()]
        public Rating Rating
        {
            get => GetValue<Rating>();
            set => SetValue(value);
        }

        [XmlArray("Titles")]
        [XmlArrayItem("Title")]
        public ObservableCollection<Title> Titles
        {
            get => GetValue<ObservableCollection<Title>>();
            set => SetValue(value);
        }

        public Episode()
        {
            Rating = new Rating();
            Rating.PropertyChanged += (s, e) => RaisePropertyChanged(() => Rating);
            Titles = new ObservableCollection<Title>();
            Titles.CollectionChanged += (s, e) => RaisePropertyChanged(() => Titles);
        }
    }
}
