﻿using Finalspace.Onigiri.Types;
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
    }
}
