using Finalspace.Onigiri.Types;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Media
{
    [Serializable]
    public class VideoInfo : IEquatable<VideoInfo>
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

        public override int GetHashCode()
            => HashCode.Combine(Name, Width, Height, FrameCount, FrameRate, Codec);

        public bool Equals(VideoInfo other)
        {
            if (other == null)
                return false;
            if (!string.Equals(Name, other.Name))
                return false;
            if (Width != other.Width)
                return false;
            if (Height != other.Height)
                return false;
            if (FrameCount != other.FrameCount)
                return false;
            if (FrameRate != other.FrameRate)
                return false;
            if (!Codec.Equals(other.Codec))
                return false;
            return true;
        }

        public override bool Equals(object obj) => Equals(obj as VideoInfo);
    }
}
