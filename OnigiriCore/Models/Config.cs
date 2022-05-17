using DevExpress.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot]
    public class Config : BindableBase
    {
        [XmlArray("SearchTypeLanguages")]
        [XmlArrayItem("SearchTypeLanguage")]
        public ObservableCollection<SearchTypeLanguage> SearchTypeLanguages { get => GetValue<ObservableCollection<SearchTypeLanguage>>(); set => SetValue(value); }

        [XmlArray("SearchPaths")]
        [XmlArrayItem("SearchPath")]
        public ObservableCollection<SearchPath> SearchPaths { get => GetValue<ObservableCollection<SearchPath>>(); set => SetValue(value); }

        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public ObservableCollection<User> Users { get => GetValue<ObservableCollection<User>>(); set => SetValue(value); }

        public int MaxThreadCount
        {
            get { return Math.Max(1, GetValue<int>()); }
            set { SetValue(value); }
        }

        public Config()
        {
            SearchTypeLanguages = new ObservableCollection<SearchTypeLanguage>();
            SearchTypeLanguages.Add(new SearchTypeLanguage() { Lang = "x-jat", Type = "main" });
            SearchTypeLanguages.Add(new SearchTypeLanguage() { Lang = "x-jat", Type = "official" });
            SearchTypeLanguages.Add(new SearchTypeLanguage() { Lang = "en", Type = "official" });

            SearchPaths = new ObservableCollection<SearchPath>();
            SearchPaths.CollectionChanged += (s, e) => RaisePropertyChanged(() => SearchPaths);

            Users = new ObservableCollection<User>();

            // TODO(final): Do not add final/anni to the users by default
            Users = new ObservableCollection<User>();
            Users.Add(new User("final", "final.png", "final_false.png"));
            Users.Add(new User("anni", "anni.png", "anni_false.png"));

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

                SearchTypeLanguages.Clear();
                foreach (SearchTypeLanguage searchTypeLang in loaded.SearchTypeLanguages)
                    SearchTypeLanguages.Add(searchTypeLang);

                SearchPaths.Clear();
                foreach (SearchPath searchPath in loaded.SearchPaths)
                    SearchPaths.Add(searchPath);

                Users.Clear();
                foreach (User user in loaded.Users)
                    Users.Add(user);

                MaxThreadCount = loaded.MaxThreadCount;
            }
        }
    }
}
