using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    public class Config : BindableBase
    {
        [XmlArrayItem("SearchPath")]
        public ObservableCollection<SearchPath> SearchPaths
        {
            get => GetValue<ObservableCollection<SearchPath>>();
            set => SetValue(value);
        }

        [XmlArrayItem("SearchTypeLanguage")]
        public ObservableCollection<SearchTypeLanguage> SearchTypeLanguages
        {
            get => GetValue< ObservableCollection<SearchTypeLanguage>>();
            private set => SetValue(value);
        }

        public int MaxThreadCount
        {
            get { return Math.Max(1, GetValue<int>()); }
            set { SetValue(value); }
        }

        public Config()
        {
            SearchPaths = new ObservableCollection<SearchPath>();
            SearchPaths.CollectionChanged += (s, e) => RaisePropertyChanged(() => SearchPaths);
            SearchTypeLanguages = new ObservableCollection<SearchTypeLanguage>();
            SearchTypeLanguages.Add(new SearchTypeLanguage() { Lang = "x-jat", Type = "main" });
            SearchTypeLanguages.Add(new SearchTypeLanguage() { Lang = "x-jat", Type = "official" });
            SearchTypeLanguages.Add(new SearchTypeLanguage() { Lang = "en", Type = "official" });
            MaxThreadCount = Math.Max(Environment.ProcessorCount - 1, 1);
        }

        public void SaveToFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(stream, this);
            }
        }

        public void LoadFromFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Config));
            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                Config loaded = (Config)serializer.Deserialize(stream);
                SearchPaths.Clear();
                foreach (SearchPath searchPath in loaded.SearchPaths)
                    SearchPaths.Add(searchPath);
            }
        }
    }
}
