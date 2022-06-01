using DevExpress.Mvvm;
using Finalspace.Onigiri.Media;
using Finalspace.Onigiri.Types;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot()]
    public class AnimeMediaFile : BindableBase, IEquatable<AnimeMediaFile>
    {
        [XmlAttribute]
        public string FileName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        [XmlAttribute]
        public ulong FileSize
        {
            get => GetValue<ulong>();
            set => SetValue(value);
        }

        [XmlElement]
        public MediaInfo Info
        {
            get => GetValue<MediaInfo>();
            set => SetValue(value);
        }

        public override int GetHashCode()
            => HashCode.Combine(FileName, FileSize, Info);

        public bool Equals(AnimeMediaFile other)
        {
            if (other == null)
                return false;
            if (!string.Equals(FileName, other.FileName))
                return false;
            if (FileSize != other.FileSize)
                return false;
            if (!Info.Equals(other.Info))
                return false;
            return true;
        }

        public override bool Equals(object obj) => Equals(obj as AnimeMediaFile);
    }
}
