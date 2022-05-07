using DevExpress.Mvvm;
using Finalspace.Onigiri.Media;
using Finalspace.Onigiri.Types;
using System;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Models
{
    [Serializable]
    [XmlRoot()]
    public class AnimeMediaFile : BindableBase
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
    }
}
