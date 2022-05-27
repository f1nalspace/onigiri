using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Types
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [XmlRoot()]
    public struct FourCC : IEquatable<FourCC>
    {
        [XmlAttribute()]
        public uint Value { get; set; }

        public bool IsEmpty => Value == 0;

        private FourCC(uint value)
        {
            Value = value;
        }

        public static FourCC FromString(string str)
        {
            if (str == null || str.Length == 0)
                return Empty;
            char[] c = new char[4];

            int len = Math.Min(c.Length, str.Length);

            int i;
            for (i = 0; i < len; i++)
                c[i] = str[i];

            for (; i < c.Length; i++)
                c[i] = ' ';

            uint value = (uint)(c[3] << 24 | c[2] << 16 | c[1] << 8 | c[0]);
            return new FourCC(value);
        }

        public override string ToString()
        {
            if (Value == 0)
                return string.Empty;
            else
            {
                char[] c = new char[4];
                c[0] = (char)(Value >> 0 & 0xFF);
                c[1] = (char)(Value >> 8 & 0xFF);
                c[2] = (char)(Value >> 16 & 0xFF);
                c[3] = (char)(Value >> 24 & 0xFF);
                string result = new string(c);
                return result;
            }
        }

        public override int GetHashCode() => Value.GetHashCode();
        public override bool Equals(object obj) => obj is FourCC fcc && Equals(fcc);

        public bool Equals(FourCC other) => Value == other.Value;

        public static bool operator ==(FourCC left, FourCC right) => left.Equals(right);
        public static bool operator !=(FourCC left, FourCC right) => !left.Equals(right);

        public static readonly FourCC Empty = new FourCC(0);

        public static FourCC FromUInt(uint value)
        {
            FourCC result = new FourCC(value);
            return result;
        }
    }
}
