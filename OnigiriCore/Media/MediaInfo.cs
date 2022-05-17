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
        public TimeSpan Duration { get => GetValue<TimeSpan>(); set => SetValue(value); }

        [XmlArray("VideoStreams")]
        [XmlArrayItem("VideoStream")]
        public List<VideoInfo> Video { get => GetValue<List<VideoInfo>>(); set => SetValue(value); }

        [XmlArray("AudioStreams")]
        [XmlArrayItem("AudioStream")]
        public List<AudioInfo> Audio { get => GetValue<List<AudioInfo>>(); set => SetValue(value); }

        [XmlArray("Subtitles")]
        [XmlArrayItem("Subtitle")]
        public List<SubtitleInfo> Subtitles { get => GetValue<List<SubtitleInfo>>(); set => SetValue(value); }

        public MediaInfo()
        {
            Format = FourCC.Empty;
            Duration = TimeSpan.Zero;
            Video = new List<VideoInfo>();
            Audio = new List<AudioInfo>();
            Subtitles = new List<SubtitleInfo>();
        }
    }
}
