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

        public override string ToString() => $"{Name} [{Id}]";

        public static readonly CodecDescription Empty = new CodecDescription(FourCC.Empty, null);
    }
}
