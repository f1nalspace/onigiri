using Finalspace.Onigiri.Types;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Media
{
    public class AudioInfo : IEquatable<AudioInfo>
    {
        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public string Lang { get; set; }

        [XmlAttribute()]
        public int Channels { get; set; }

        [XmlAttribute()]
        public int SampleRate { get; set; }

        [XmlAttribute()]
        public int BitsPerSample { get; set; }

        [XmlAttribute()]
        public long BitRate { get; set; }

        [XmlAttribute()]
        public long FrameCount { get; set; }

        [XmlElement()]
        public CodecDescription Codec { get; set; }

        public AudioInfo()
        {
            Name = null;
            Lang = null;
            Channels = 0;
            SampleRate = 0;
            BitsPerSample = 0;
            BitRate = 0;
            FrameCount = 0;
            Codec = CodecDescription.Empty;
        }

        public override int GetHashCode()
            => HashCode.Combine(Name, Lang, Channels, SampleRate, BitsPerSample, BitRate, FrameCount, Codec);

        public bool Equals(AudioInfo other)
        {
            if (other == null)
                return false;
            if (!string.Equals(Name, other.Name))
                return false;
            if (!string.Equals(Lang, other.Lang))
                return false;
            if (Channels != other.Channels)
                return false;
            if (SampleRate != other.SampleRate)
                return false;
            if (BitsPerSample != other.BitsPerSample)
                return false;
            if (BitRate != other.BitRate)
                return false;
            if (FrameCount != other.FrameCount)
                return false;
            if (!Codec.Equals(other.Codec))
                return false;
            return true;
        }

        public override bool Equals(object obj) => Equals(obj as AudioInfo);
    }
}
