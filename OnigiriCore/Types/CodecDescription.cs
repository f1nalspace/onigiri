using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Types
{
    [Serializable]
    public readonly struct CodecDescription
    {
        [XmlAttribute]
        public FourCC Id { get; }

        [XmlAttribute]
        public string Name { get; }

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
