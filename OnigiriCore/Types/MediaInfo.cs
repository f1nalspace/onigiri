using System;
using System.Collections.Immutable;

namespace Finalspace.Onigiri.Types
{
    public class MediaInfo
    {
        public FourCC Format { get; }
        public VideoInfo Video { get; }
        public ImmutableArray<AudioInfo> Audio { get; }
        public ImmutableArray<SubtitleInfo> Subtitles { get; }

        public MediaInfo(FourCC format, VideoInfo video, ImmutableArray<AudioInfo> audio, ImmutableArray<SubtitleInfo> subtitles)
        {
            if (audio == null)
                throw new ArgumentNullException(nameof(audio));
            if (subtitles == null)
                throw new ArgumentNullException(nameof(subtitles));
            Format = format;
            Video = video;
            Audio = audio;
            Subtitles = subtitles;
        }
    }
}
