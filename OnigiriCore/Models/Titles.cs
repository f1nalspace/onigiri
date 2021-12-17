using Finalspace.Onigiri.Utils;
using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;
using Finalspace.Onigiri.MVVM;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot(ElementName = "Titles")]
    public class Titles : BindableBase
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [XmlArray("Items")]
        [XmlArrayItem("Item")]
        public ObservableCollection<Title> Items
        {
            get { return GetValue(() => Items); }
            set { SetValue(() => Items, value); }
        }
        private readonly Dictionary<ulong, List<Title>> _aidToTitlesMap;

        public int AIDCount
        {
            get
            {
                return _aidToTitlesMap.Count;
            }
        }

        public IEnumerable<Title> GetTitlesByAID(ulong aid)
        {
            if (_aidToTitlesMap.ContainsKey(aid))
            {
                return _aidToTitlesMap[aid];
            }
            return null;
        }

        public Titles()
        {
            Items = new ObservableCollection<Title>();
            Items.CollectionChanged += (s, e) => RaisePropertyChanged(() => Items);
            _aidToTitlesMap = new Dictionary<ulong, List<Title>>();
        }

        public Title FindTitle(string title, string type, string lang)
        {
            IEnumerable<Title> matched = Items.Where((d) =>
                d.Name.Equals(title, StringComparison.InvariantCultureIgnoreCase)
                && d.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase)
                && d.Lang.Equals(lang, StringComparison.InvariantCultureIgnoreCase));
            Title result = matched.FirstOrDefault();
            return(result);
        }

        public Title GetTitle(ulong aid, string type = "main", string lang = null)
        {
            IEnumerable<Title> matched = Items.Where((d) => d.Aid == aid && d.Type.Equals(type, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrEmpty(lang) || d.Lang.Equals(lang, StringComparison.InvariantCultureIgnoreCase)));
            Title result = matched.FirstOrDefault();
            if (result != null)
                return result;
            return null;
        }

        public void Add(Title title)
        {
            Items.Add(title);
            ulong aid = title.Aid;
            if (aid > 0)
            {
                // Aid to titles
                List<Title> aidTitles;
                if (!_aidToTitlesMap.ContainsKey(aid))
                {
                    aidTitles = new List<Title>();
                    _aidToTitlesMap.Add(aid, aidTitles);
                }
                else
                    aidTitles = _aidToTitlesMap[aid];
                aidTitles.Add(title);
            }
        }

        public void ReadFromFile(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                using (StreamReader inputStream = new StreamReader(filePath, Encoding.UTF8))
                {
                    doc.Load(inputStream);
                    XmlNode rootNode = doc.SelectSingleNode("animetitles");
                    if (rootNode != null)
                    {
                        XmlNodeList animeNodes = rootNode.SelectNodes("anime");
                        foreach (XmlNode animeNode in animeNodes)
                        {
                            ulong aid = XMLUtils.GetAttribute<ulong>(animeNode, "aid", 0);
                            XmlNodeList titleNodes = animeNode.SelectNodes("title");
                            foreach (XmlNode titleNode in titleNodes)
                            {
                                string type = XMLUtils.GetAttribute(titleNode, "type", string.Empty);
                                string lang = XMLUtils.GetAttribute(titleNode, "xml:lang", string.Empty);
                                string name = titleNode.InnerText;
                                Title title = new Title()
                                {
                                    Aid = aid,
                                    Lang = lang,
                                    Type = type,
                                    Name = name
                                };
                                Add(title);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed reading anime titles xml file '{filePath}'!", e);
            }
        }
    }
}