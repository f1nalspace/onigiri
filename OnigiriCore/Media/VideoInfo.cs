using Finalspace.Onigiri.Types;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Media
{
    [Serializable]
    public class VideoInfo
    {
        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public int Width { get; set; }

        [XmlAttribute()]
        public int Height { get; set; }

        [XmlAttribute()]
        public long FrameCount { get; set; }

        [XmlAttribute()]
        public double FrameRate { get; set; }

        [XmlElement()]
        public CodecDescription Codec { get; set; }

        public VideoInfo()
        {
            Name = null;
            Width = 0;
            Height = 0;
            FrameCount = 0;
            FrameRate = 0;
            Codec = CodecDescription.Empty;
        }

        public VideoInfo(string name, int width, int height, int frameCount, double frameRate, CodecDescription codec)
        {
            Name = name;
            Width = width;
            Height = height;
            FrameCount = frameCount;
            FrameRate = frameRate;
            Codec = codec;
        }
    }
}
