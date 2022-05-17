using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot(ElementName = "additionaldata")]
    public class AdditionalData : BindableBase
    {
        [XmlArray("watchstates")]
        [XmlArrayItem("watched")]
        public ObservableCollection<UserState> Watchstates
        {
            get => GetValue<ObservableCollection<UserState>>();
            private set => SetValue(value, RaiseWatchStatesChanged);
        }
        [XmlArray("deleteits")]
        [XmlArrayItem("deleteit")]
        public ObservableCollection<UserState> Deleteits
        {
            get => GetValue<ObservableCollection<UserState>>();
            private set => SetValue(value, RaiseDeleteitsChanged);
        }

        public bool HasAnniWatched
        {
            get
            {
                UserState first = Watchstates.Where((d) => "anni".Equals(d.UserName)).FirstOrDefault();
                if (first != null)
                    return first.Value;
                return false;
            }
        }
        public bool HasFinalWatched
        {
            get
            {
                UserState first = Watchstates.Where((d) => "final".Equals(d.UserName)).FirstOrDefault();
                if (first != null)
                    return first.Value;
                return false;
            }
        }
        public bool HasFinalUnwatched => !HasFinalWatched;
        public bool HasAnniUnwatched => !HasAnniWatched;
        public bool HasBothUnwatched => !HasFinalWatched && !HasAnniWatched;
        public bool HasAnniDeleteit
        {
            get
            {
                UserState first = Deleteits.Where((d) => "anni".Equals(d.UserName)).FirstOrDefault();
                if (first != null)
                    return first.Value;
                return false;
            }
        }
        public bool HasFinalDeleteit
        {
            get
            {
                UserState first = Deleteits.Where((d) => "final".Equals(d.UserName)).FirstOrDefault();
                if (first != null)
                    return first.Value;
                return false;
            }
        }

        private void RaiseWatchStatesChanged()
        {
            RaisePropertyChanged(nameof(HasAnniWatched));
            RaisePropertyChanged(nameof(HasFinalWatched));
        }
        private void RaiseDeleteitsChanged()
        {
            RaisePropertyChanged(nameof(HasAnniDeleteit));
            RaisePropertyChanged(nameof(HasFinalDeleteit));
        }

        [XmlElement("marked")]
        public bool Marked
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        public AdditionalData()
        {
            Watchstates = new ObservableCollection<UserState>();
            Watchstates.CollectionChanged += (s, e) => RaisePropertyChanged(() => Watchstates);
            Deleteits = new ObservableCollection<UserState>();
            Deleteits.CollectionChanged += (s, e) => RaisePropertyChanged(() => Deleteits);
        }

        public void ToggleWatchState(string who)
        {
            UserState first = Watchstates.FirstOrDefault((d) => who.Equals(d.UserName));
            if (first == null)
            {
                first = new UserState() { UserName = who, Value = true };
                Watchstates.Add(first);
            }
            else
                first.Value = !first.Value;
            RaiseWatchStatesChanged();
        }

        public void ToggleDeleteit(string who)
        {
            UserState first = Deleteits.FirstOrDefault((d) => who.Equals(d.UserName));
            if (first == null)
            {
                first = new UserState() { UserName = who, Value = true };
                Deleteits.Add(first);
            }
            else
                first.Value = !first.Value;
            RaiseDeleteitsChanged();
        }

        public void SaveToFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AdditionalData));
            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(stream, this);
            }
        }

        public void LoadFromFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AdditionalData));
            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                AdditionalData loaded = (AdditionalData)serializer.Deserialize(stream);
                Watchstates.Clear();
                foreach (UserState itm in loaded.Watchstates)
                    Watchstates.Add(itm);
                foreach (UserState itm in loaded.Deleteits)
                    Deleteits.Add(itm);
                Marked = loaded.Marked;
            }
        }
    }
}
