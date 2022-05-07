using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Media
{
    [Serializable]
    public struct SubtitleInfo
    {
        [XmlAttribute()]
        public string Lang { get; set; }

        [XmlAttribute()]
        public string DisplayName { get; set; }

        public SubtitleInfo(string language, string displayName)
        {
            Lang = language;
            DisplayName = displayName;
        }
    }
}
