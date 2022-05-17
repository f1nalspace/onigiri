using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Media
{
    [Serializable]
    public class SubtitleInfo
    {
        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public string Lang { get; set; }


        public SubtitleInfo()
        {
            Name = null;
            Lang = null;
        }

        public SubtitleInfo(string name, string language)
        {
            Name = name;
            Lang = language;
        }
    }
}
