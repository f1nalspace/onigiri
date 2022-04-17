using Finalspace.Onigiri.Utils;
using log4net;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;
using System.Collections.Generic;
using DevExpress.Mvvm;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot()]
    public class Anime : BindableBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static string UniversalDateFormat = "yyyy-MM-dd";

        [XmlElement]
        public AdditionalData AddonData
        {
            get => GetValue<AdditionalData>();
            set => SetValue(value);
        }

        [XmlArray("MediaFiles")]
        [XmlArrayItem("MediaFile")]
        public List<string> MediaFiles
        {
            get => GetValue<List<string>>();
            set => SetValue(value, () => RaisePropertyChanged(nameof(MediaFileCount)));
        }

        public int MediaFileCount => MediaFiles.Count;

        [XmlElement]
        public string FoundPath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlElement]
        public string ImageFilePath
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute]
        public bool Restricted
        {
            get => GetValue<bool>();
            set => SetValue(value);
        }

        [XmlAttribute]
        public ulong Aid
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        [XmlAttribute]
        public string Type
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute]
        public int EpCount
        {
            get => GetValue<int>();
            set => SetValue(value);
        }

        [XmlElement]
        public DateTime? StartDate
        {
            get => GetValue<DateTime?>();
            set => SetValue(value);
        }

        [XmlElement]
        public DateTime? EndDate
        {
            get => GetValue<DateTime?>();
            set => SetValue(value);
        }

        [XmlElement]
        public string Description
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlElement]
        public string Picture
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlElement("Titles")]
        public Titles Titles
        {
            get => GetValue<Titles>();
            set => SetValue(value);
        }

        [XmlArray("Ratings")]
        [XmlArrayItem("Rating")]
        public ObservableCollection<Rating> Ratings
        {
            get => GetValue<ObservableCollection<Rating>>();
            set => SetValue(value);
        }

        [XmlArray("Categories")]
        [XmlArrayItem("Category")]
        public ObservableCollection<Category> Categories
        {
            get => GetValue<ObservableCollection<Category>>();
            set => SetValue(value);
        }

        [XmlArray("Episodes")]
        [XmlArrayItem("Episode")]
        public ObservableCollection<Episode> Episodes
        {
            get => GetValue<ObservableCollection<Episode>>();
            set => SetValue(value);
        }

        [XmlArray("TopCategories")]
        [XmlArrayItem("Category")]
        public ObservableCollection<Category> TopCategories
        {
            get => GetValue<ObservableCollection<Category>>();
            set => SetValue(value);
        }

        [XmlArray("Relations")]
        [XmlArrayItem("Relation")]
        public ObservableCollection<Relation> Relations
        {
            get => GetValue<ObservableCollection<Relation>>();
            set => SetValue(value);
        }

        public bool IsSequal(ulong aid)
        {
            int count = Relations.Count(r => r.Type == RelationType.Prequel && r.Aid == aid);
            bool result = (count == 1);
            return (result);
        }

        public string MainTitle
        {
            get
            {
                Title title = Titles.GetTitle(Aid);
                if (title != null)
                    return title.Name;
                return null;
            }
        }

        public double PermanentRating
        {
            get
            {
                Rating first = Ratings.Where((d) => "permanent".Equals(d.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (first != null)
                    return first.Value;
                return 0.0;
            }
        }
        public double TemporaryRating
        {
            get
            {
                Rating first = Ratings.Where((d) => "temporary".Equals(d.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (first != null)
                    return first.Value;
                return 0.0;
            }
        }

        [XmlIgnore]
        public AnimeImage Image {
            get => GetValue<AnimeImage>(); 
            set => SetValue(value); }

        public Anime()
        {
            Titles = new Titles();
            Titles.PropertyChanged += (s, e) =>
            {
                RaisePropertyChanged(() => Titles);
                RaisePropertyChanged(() => MainTitle);
            };
            Ratings = new ObservableCollection<Models.Rating>();
            Ratings.CollectionChanged += (s, e) =>
            {
                RaisePropertyChanged(() => Ratings);
                RaisePropertyChanged(() => PermanentRating);
            };
            Categories = new ObservableCollection<Category>();
            Categories.CollectionChanged += (s, e) => RaisePropertyChanged(() => Categories);
            TopCategories = new ObservableCollection<Category>();
            TopCategories.CollectionChanged += (s, e) => RaisePropertyChanged(() => TopCategories);
            Episodes = new ObservableCollection<Episode>();
            Episodes.CollectionChanged += (s, e) => RaisePropertyChanged(() => Episodes);
            Relations = new ObservableCollection<Relation>();
            Relations.CollectionChanged += (s, e) => RaisePropertyChanged(() => Relations);
            AddonData = new AdditionalData();
            AddonData.PropertyChanged += (s, e) => RaisePropertyChanged(() => AddonData);

            Image = null;
        }

        public void LoadFromAnimeXML(string filePath, bool skipDetails = false)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(filePath);
                XmlNode rootNode = doc.SelectSingleNode("anime");
                if (rootNode != null)
                {
                    try
                    {
                        Aid = XMLUtils.GetAttribute<ulong>(rootNode, "id", 0);
                        Restricted = XMLUtils.GetAttribute(rootNode, "restricted", false);
                        Type = XMLUtils.GetValue(rootNode, "type", string.Empty);
                        EpCount = XMLUtils.GetValue<int>(rootNode, "episodecount", 0);
                        StartDate = XMLUtils.GetValue<DateTime?>(rootNode, "startdate", null, UniversalDateFormat);
                        EndDate = XMLUtils.GetValue<DateTime?>(rootNode, "enddate", null, UniversalDateFormat);
                        Description = XMLUtils.GetValue(rootNode, "description", string.Empty);
                        Picture = XMLUtils.GetValue(rootNode, "picture", string.Empty);
                    }
                    catch (Exception e1)
                    {
                        throw new IOException($"Failed reading root infos from file '{filePath}'!", e1);
                    }

                    if (!skipDetails)
                    {
                        // Titles
                        try
                        {
                            XmlNodeList titleNodes = rootNode.SelectNodes("titles/title");
                            foreach (XmlNode titleNode in titleNodes)
                            {
                                string lang = XMLUtils.GetAttribute(titleNode, "xml:lang", string.Empty);
                                string type = XMLUtils.GetAttribute(titleNode, "type", string.Empty);
                                string name = titleNode.InnerText;
                                Titles.Add(new Title()
                                {
                                    Aid = Aid,
                                    Lang = lang,
                                    Type = type,
                                    Name = name
                                });
                            }
                        }
                        catch (Exception e1)
                        {
                            throw new IOException($"Failed reading titles from file '{filePath}'!", e1);
                        }

                        // Categories
                        try
                        {
                            XmlNodeList catNodes = rootNode.SelectNodes("categories/category");
                            foreach (XmlNode catNode in catNodes)
                            {
                                ulong id = XMLUtils.GetAttribute<ulong>(catNode, "id", 0);
                                ulong parentId = XMLUtils.GetAttribute<ulong>(catNode, "parentid", 0);
                                int weight = XMLUtils.GetAttribute<int>(catNode, "weight", 0);
                                string name = XMLUtils.GetValue(catNode, "name", string.Empty);
                                string desc = XMLUtils.GetValue(catNode, "description", string.Empty);
                                Categories.Add(new Category()
                                {
                                    Name = name,
                                    Description = desc,
                                    Id = id,
                                    ParentId = parentId,
                                    Weight = weight
                                });
                            }
                        }
                        catch (Exception e1)
                        {
                            throw new IOException($"Failed reading titles from file '{filePath}'!", e1);
                        }

                        // Top categories
                        IEnumerable<Category> topCats = Categories.OrderByDescending(d => d.Weight).Take(10);
                        foreach (Category item in topCats)
                            TopCategories.Add(item);

                        // Ratings
                        try
                        {
                            XmlNodeList ratingNodes = rootNode.SelectNodes("ratings/*");
                            foreach (XmlNode ratingNode in ratingNodes)
                            {
                                string name = ratingNode.Name;
                                ulong count = XMLUtils.GetAttribute<ulong>(ratingNode, "count", 0);
                                double rating = double.Parse(ratingNode.InnerText, CultureInfo.InvariantCulture);
                                Ratings.Add(new Rating()
                                {
                                    Count = count,
                                    Name = name,
                                    Value = rating
                                });
                            }
                        }
                        catch (Exception e1)
                        {
                            throw new IOException($"Failed reading ratings from file '{filePath}'!", e1);
                        }

                        // Episodes
                        try
                        {
                            XmlNodeList episodeNodes = rootNode.SelectNodes("episodes/episode");
                            foreach (XmlNode episodeNode in episodeNodes)
                            {
                                ulong eid = XMLUtils.GetAttribute<ulong>(episodeNode, "id", 0);
                                DateTime? upDate = XMLUtils.GetAttribute<DateTime?>(episodeNode, "update", null, UniversalDateFormat);
                                string epNo = XMLUtils.GetValue(episodeNode, "epno", string.Empty);
                                ulong epNoType = XMLUtils.GetAttribute<ulong>(episodeNode, "epno", "type", 0);
                                ulong length = XMLUtils.GetValue<ulong>(episodeNode, "length", 0);
                                DateTime? airDate = XMLUtils.GetValue<DateTime?>(episodeNode, "airdate", null, UniversalDateFormat);
                                double epRating = XMLUtils.GetValue<double>(episodeNode, "rating", 0);
                                ulong epRatingCount = XMLUtils.GetAttribute<ulong>(episodeNode, "rating", "votes", 0);

                                Episode episode = new Episode()
                                {
                                    Id = eid,
                                    AirDate = airDate,
                                    Length = length,
                                    Num = epNo,
                                    NumType = epNoType,
                                };
                                episode.Rating.Value = epRating;
                                episode.Rating.Count = epRatingCount;
                                Episodes.Add(episode);

                                // Episode titles
                                XmlNodeList epTitleNodes = episodeNode.SelectNodes("titles");
                                foreach (XmlNode epTitleNode in epTitleNodes)
                                {
                                    string name = epTitleNode.InnerText;
                                    string lang = XMLUtils.GetAttribute(epTitleNode, "xml:lang", string.Empty);
                                    Title title = new Title()
                                    {
                                        Lang = lang,
                                        Name = name
                                    };
                                    episode.Titles.Add(title);
                                }
                            }
                        }
                        catch (Exception e1)
                        {
                            throw new IOException($"Failed reading episodes from file '{filePath}'!", e1);
                        }

                        // Relations
                        try
                        {
                            XmlNodeList relationNodes = rootNode.SelectNodes("relatedanime/anime");
                            foreach (XmlNode relationNode in relationNodes)
                            {
                                ulong relatedAid = XMLUtils.GetAttribute<ulong>(relationNode, "id", 0);
                                string typeStr = XMLUtils.GetAttribute<string>(relationNode, "type", null);
                                string relatedName = relationNode.InnerText;
                                Relations.Add(new Relation()
                                {
                                    Aid = relatedAid,
                                    Name = relatedName,
                                    TypeStr = typeStr,
                                });
                            }
                        }
                        catch (Exception e1)
                        {
                            throw new IOException($"Failed reading ratings from file '{filePath}'!", e1);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed loading anime details from file '{filePath}'!", e);
            }
        }

        public override string ToString()
        {
            return $"{MainTitle} (aid={Aid}, type={Type}, eps={EpCount})";
        }

    }
}

