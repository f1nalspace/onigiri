using Finalspace.Onigiri.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace Finalspace.Onigiri.Media.Parsers
{
    class FFMpegMediaInfoParser : IMediaInfoParser
    {
        public async Task<MediaInfo> Parse(string filePath)
        {
            var info = await FFmpeg.GetMediaInfo(filePath);
            if (info == null)
                return null;

            string ext = Path.GetExtension(filePath).TrimStart('.');

            FourCC format = FourCC.FromString(ext);

            MediaInfo result = new MediaInfo()
            {
                Format = format,
                Duration = info.Duration,
            };

            foreach (var sourceVideo in info.VideoStreams)
            {
                VideoInfo targetVideo = new VideoInfo()
                {
                    Codec = new CodecDescription(FourCC.Empty, sourceVideo.Codec),
                    Name = $"Video[{sourceVideo.Index}]",
                    Width = sourceVideo.Width,
                    Height = sourceVideo.Height,
                    FrameRate = sourceVideo.Framerate,
                    FrameCount = (int)(sourceVideo.Duration.TotalMilliseconds / sourceVideo.Framerate),
                };
                result.Video.Add(targetVideo);
            }

            foreach (var sourceAudio in info.AudioStreams)
            {

                AudioInfo targetAudio = new AudioInfo()
                {
                    Codec = new CodecDescription(FourCC.Empty, sourceAudio.Codec),
                    Name = $"Audio[{sourceAudio.Index}]{(!string.IsNullOrWhiteSpace(sourceAudio.Title) ? $": {sourceAudio.Title}" : "")}",
                    Channels = (uint)sourceAudio.Channels,
                    Lang = sourceAudio.Language,
                    SampleRate = (uint)sourceAudio.SampleRate,
                    BitRate = (uint)sourceAudio.Bitrate,
                };
                result.Audio.Add(targetAudio);
            }

            foreach (var sourceSubtitle in info.SubtitleStreams)
            {
                SubtitleInfo targetSubtitle = new SubtitleInfo()
                {
                    Name = $"Subtitle[{sourceSubtitle.Index}]{(!string.IsNullOrWhiteSpace(sourceSubtitle.Title) ? $": {sourceSubtitle.Title}" : "")}",
                    Lang = sourceSubtitle.Language,
                };
                result.Subtitles.Add(targetSubtitle);
            }

            return result;
        }
    }
}
