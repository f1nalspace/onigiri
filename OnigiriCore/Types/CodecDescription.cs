using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Types
{
    [Serializable]
    [XmlRoot]
    public struct CodecDescription : IEquatable<CodecDescription>
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

        public override int GetHashCode() => HashCode.Combine(Id, Name);

        public bool Equals(CodecDescription other)
            => string.Equals(Name, other.Name) && Id.Equals(other.Id);

        public override bool Equals(object obj) => obj is CodecDescription codecDesc && Equals(codecDesc);
        public static bool operator ==(CodecDescription left, CodecDescription right) => left.Equals(right);
        public static bool operator !=(CodecDescription left, CodecDescription right) => !(left == right);

        public static readonly CodecDescription Empty = new CodecDescription(FourCC.Empty, null);
    }
}
