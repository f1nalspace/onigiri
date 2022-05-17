using DevExpress.Mvvm;
using Finalspace.Onigiri.Types;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Media
{
    [Serializable]
    [XmlRoot()]
    public class MediaInfo : BindableBase
    {
        [XmlElement]
        public FourCC Format { get => GetValue<FourCC>(); set => SetValue(value); }

        [XmlElement]
        public VideoInfo Video { get => GetValue<VideoInfo>(); set => SetValue(value); }

        [XmlArray("AudioInfos")]
        [XmlArrayItem("AudioInfo")]
        public List<AudioInfo> Audio { get => GetValue<List<AudioInfo>>(); set => SetValue(value); }

        [XmlArray("SubtitleInfos")]
        [XmlArrayItem("SubtitleInfo")]
        public List<SubtitleInfo> Subtitles { get => GetValue<List<SubtitleInfo>>(); set => SetValue(value); }

        public MediaInfo()
        {
            Audio = new List<AudioInfo>();
            Subtitles = new List<SubtitleInfo>();
        }
    }
}
