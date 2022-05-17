using Finalspace.Onigiri.Types;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Media
{
    public class AudioInfo
    {
        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public string Lang { get; set; }

        [XmlAttribute()]
        public uint Channels { get; set; }

        [XmlAttribute()]
        public uint SampleRate { get; set; }

        [XmlAttribute()]
        public uint BytesPerSample { get; set; }

        [XmlElement()]
        public CodecDescription Codec { get; set; }

        public AudioInfo()
        {
            Name = null;
            Lang = null;
            Channels = 0;
            SampleRate = 0;
            BytesPerSample = 0;
            Codec = CodecDescription.Empty;
        }

        public AudioInfo(string name, string language, uint channels, uint sampleRate, uint bytesPerSample, CodecDescription codec)
        {
            Name = name;
            Lang = language;
            Channels = channels;
            SampleRate = sampleRate;
            BytesPerSample = bytesPerSample;
            Codec = codec;
        }
    }
}
