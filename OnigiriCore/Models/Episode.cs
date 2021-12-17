using Finalspace.Onigiri.MVVM;
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
            get { return GetValue(() => Id); }
            set { SetValue(() => Id, value); }
        }

        [XmlAttribute()]
        public string Num
        {
            get { return GetValue(() => Num); }
            set { SetValue(() => Num, value); }
        }

        [XmlAttribute()]
        public ulong NumType
        {
            get { return GetValue(() => NumType); }
            set { SetValue(() => NumType, value); }
        }

        [XmlAttribute()]
        public ulong Length
        {
            get { return GetValue(() => Length); }
            set { SetValue(() => Length, value); }
        }

        [XmlElement()]
        public DateTime? AirDate
        {
            get { return GetValue(() => AirDate); }
            set { SetValue(() => AirDate, value); }
        }

        [XmlElement()]
        public DateTime? UpDate
        {
            get { return GetValue(() => UpDate); }
            set { SetValue(() => UpDate, value); }
        }

        [XmlElement()]
        public Rating Rating
        {
            get { return GetValue(() => Rating); }
            set { SetValue(() => Rating, value); }
        }

        [XmlArray("Titles")]
        [XmlArrayItem("Title")]
        public ObservableCollection<Title> Titles
        {
            get { return GetValue(() => Titles); }
            set { SetValue(() => Titles, value); }
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
