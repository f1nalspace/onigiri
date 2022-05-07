using Finalspace.Onigiri.Types;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Media
{
    [Serializable]
    public struct VideoInfo
    {
        [XmlAttribute()]
        public int Width { get; set; }

        [XmlAttribute()]
        public int Height { get; set; }

        [XmlAttribute()]
        public double FrameRate { get; set; }

        [XmlElement()]
        public FourCC Codec { get; set; }

        public VideoInfo(int width, int height, double frameRate, FourCC codec)
        {
            Width = width;
            Height = height;
            FrameRate = frameRate;
            Codec = codec;
        }
    }
}
