using Finalspace.Onigiri.MVVM;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot(ElementName = "additionaldata")]
    public class AdditionalData: BindableBase
    {
        [XmlArray("watchstates")]
        [XmlArrayItem("watched")]
        public ObservableCollection<NameState> Watchstates
        {
            get { return GetValue(() => Watchstates); }
            private set { SetValue(() => Watchstates, value, RaiseWatchStatesChanged); }
        }
        [XmlArray("deleteits")]
        [XmlArrayItem("deleteit")]
        public ObservableCollection<NameState> Deleteits
        {
            get { return GetValue(() => Deleteits); }
            private set { SetValue(() => Deleteits, value, RaiseDeleteitsChanged); }
        }

        public bool HasAnniWatched
        {
            get
            {
                NameState first = Watchstates.Where((d) => "anni".Equals(d.Name)).FirstOrDefault();
                if (first != null)
                    return first.Value;
                return false;
            }
        }
        public bool HasFinalWatched
        {
            get
            {
                NameState first = Watchstates.Where((d) => "final".Equals(d.Name)).FirstOrDefault();
                if (first != null)
                    return first.Value;
                return false;
            }
        }
        public bool HasFinalUnwatched
        {
            get { return !HasFinalWatched; }
        }
        public bool HasAnniUnwatched
        {
            get { return !HasAnniWatched; }
        }
        public bool HasBothUnwatched
        {
            get
            {
                
                return !HasFinalWatched && !HasAnniWatched;
            }
        }
        public bool HasAnniDeleteit
        {
            get
            {
                NameState first = Deleteits.Where((d) => "anni".Equals(d.Name)).FirstOrDefault();
                if (first != null)
                    return first.Value;
                return false;
            }
        }
        public bool HasFinalDeleteit
        {
            get
            {
                NameState first = Deleteits.Where((d) => "final".Equals(d.Name)).FirstOrDefault();
                if (first != null)
                    return first.Value;
                return false;
            }
        }

        private void RaiseWatchStatesChanged()
        {
            RaisePropertyChanged(() => HasAnniWatched);
            RaisePropertyChanged(() => HasFinalWatched);
        }
        private void RaiseDeleteitsChanged()
        {
            RaisePropertyChanged(() => HasAnniDeleteit);
            RaisePropertyChanged(() => HasFinalDeleteit);
        }

        [XmlElement("marked")]
        public bool Marked
        {
            get { return GetValue(() => Marked); }
            set { SetValue(() => Marked, value); }
        }

        public AdditionalData()
        {
            Watchstates = new ObservableCollection<NameState>();
            Watchstates.CollectionChanged += (s, e) => RaisePropertyChanged(() => Watchstates);
            Deleteits = new ObservableCollection<NameState>();
            Deleteits.CollectionChanged += (s, e) => RaisePropertyChanged(() => Deleteits);
        }

        public void ToggleWatchState(string who)
        {
            NameState first = Watchstates.FirstOrDefault((d) => who.Equals(d.Name));
            if (first == null)
            {
                first = new NameState() { Name = who, Value = true };
                Watchstates.Add(first);
            }
            else
                first.Value = !first.Value;
            RaiseWatchStatesChanged();
        }

        public void ToggleDeleteit(string who)
        {
            NameState first = Deleteits.FirstOrDefault((d) => who.Equals(d.Name));
            if (first == null)
            {
                first = new NameState() { Name = who, Value = true };
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
                foreach (NameState itm in loaded.Watchstates)
                    Watchstates.Add(itm);
                foreach (NameState itm in loaded.Deleteits)
                    Deleteits.Add(itm);
                Marked = loaded.Marked;
            }
        }
    }
}
