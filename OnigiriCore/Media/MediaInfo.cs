using DevExpress.Mvvm;
using Finalspace.Onigiri.Types;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Media
{
    [Serializable]
    [XmlRoot()]
    public class MediaInfo : BindableBase, IEquatable<MediaInfo>
    {
        [XmlElement]
        public CodecDescription Format { get => GetValue<CodecDescription>(); set => SetValue(value); }

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
            Format = CodecDescription.Empty;
            Duration = TimeSpan.Zero;
            Video = new List<VideoInfo>();
            Audio = new List<AudioInfo>();
            Subtitles = new List<SubtitleInfo>();
        }

        public override int GetHashCode()
        {
            const int HashPrime = 31;

            unchecked
            {
                int result = 17;

                result *= HashPrime + Format.GetHashCode();
                result *= HashPrime + Duration.GetHashCode();

                int videoCount = Video.Count;
                int audioCount = Audio.Count;
                int subtitleCount = Subtitles.Count;

                result *= HashPrime + videoCount.GetHashCode();
                foreach (var video in Video)
                    result *= HashPrime + video.GetHashCode();

                result *= HashPrime + audioCount.GetHashCode();
                foreach (var audio in Audio)
                    result *= HashPrime + audio.GetHashCode();

                result *= HashPrime + subtitleCount.GetHashCode();
                foreach (var subtitle in Subtitles)
                    result *= HashPrime + subtitle.GetHashCode();

                return result;
            }
        }

        public bool Equals(MediaInfo other)
        {
            if (other == null)
                return false;
            if (!Format.Equals(other.Format))
                return false;
            if (!Duration.Equals(other.Duration))
                return false;

            if (Video.Count != other.Video.Count)
                return false;
            for (int index = 0; index < Video.Count; index++)
            {
                VideoInfo source = Video[index];
                VideoInfo target = other.Video[index];
                if (!source.Equals(target))
                    return false;
            }

            if (Audio.Count != other.Audio.Count)
                return false;
            for (int index = 0; index < Audio.Count; index++)
            {
                AudioInfo source = Audio[index];
                AudioInfo target = other.Audio[index];
                if (!source.Equals(target))
                    return false;
            }

            if (Subtitles.Count != other.Subtitles.Count)
                return false;
            for (int index = 0; index < Subtitles.Count; index++)
            {
                SubtitleInfo source = Subtitles[index];
                SubtitleInfo target = other.Subtitles[index];
                if (!source.Equals(target))
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj) => Equals(obj as MediaInfo);
    }
}
