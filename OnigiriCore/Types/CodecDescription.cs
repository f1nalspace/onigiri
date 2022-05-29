using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Types
{
    [Serializable]
    [XmlRoot]
    public struct CodecDescription
    {
        [XmlElement]
        public FourCC Id { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        public CodecDescription(FourCC id, string name)
        {
            Id = id;
            Name = name;
        }

        public CodecDescription(FourCC id)
        {
            Id = id;
            Name = id.ToString();
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(Name) && !Id.IsEmpty)
                return $"{Name} [{Id}]";
            else if (!string.IsNullOrWhiteSpace(Name) && Id.IsEmpty)
                return $"{Name}";
            else if (string.IsNullOrWhiteSpace(Name) && !Id.IsEmpty)
                return $"{Id}";
            else
                return null;
        }

        public static readonly CodecDescription Empty = new CodecDescription(FourCC.Empty, null);
    }
}
