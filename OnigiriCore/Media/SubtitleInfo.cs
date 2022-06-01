using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Media
{
    [Serializable]
    public class SubtitleInfo : IEquatable<SubtitleInfo>
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

        public override int GetHashCode()
            => HashCode.Combine(Name, Lang);

        public bool Equals(SubtitleInfo other)
        {
            if (other == null)
                return false;
            if (!string.Equals(Name, other.Name))
                return false;
            if (!string.Equals(Lang, other.Lang))
                return false;
            return true;
        }

        public override bool Equals(object obj) => Equals(obj as SubtitleInfo);
    }
}
