using FFmpeg.AutoGen;
using Finalspace.Onigiri.Types;
using Finalspace.Onigiri.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using static FFmpeg.AutoGen.AVMediaType;
using static FFmpeg.AutoGen.ffmpeg;

namespace Finalspace.Onigiri.Media.Parsers
{
    unsafe class FFMpegMediaInfoParser : IMediaInfoParser
    {
        public FFMpegMediaInfoParser()
        {
            var asm = Assembly.GetEntryAssembly();
            var appPath = Path.GetDirectoryName(asm.Location);
            var path = Path.Combine(appPath, "ffmpeg-x64");
            ffmpeg.RootPath = path;
            ffmpeg.av_log_set_level(AV_LOG_QUIET);
        }

        public Task<MediaInfo> Parse(string filePath) => Task.Run(() =>
        {
            int GetBitsPerSample(AVSampleFormat format)
            {
                switch (format)
                {
                    case AVSampleFormat.AV_SAMPLE_FMT_U8:
                    case AVSampleFormat.AV_SAMPLE_FMT_U8P:
                        return 8;
                    case AVSampleFormat.AV_SAMPLE_FMT_S16:
                    case AVSampleFormat.AV_SAMPLE_FMT_S16P:
                        return 16;
                    case AVSampleFormat.AV_SAMPLE_FMT_S32:
                    case AVSampleFormat.AV_SAMPLE_FMT_S32P:
                        return 32;
                    case AVSampleFormat.AV_SAMPLE_FMT_FLT:
                    case AVSampleFormat.AV_SAMPLE_FMT_FLTP:
                        return 32;
                    case AVSampleFormat.AV_SAMPLE_FMT_DBL:
                    case AVSampleFormat.AV_SAMPLE_FMT_DBLP:
                    case AVSampleFormat.AV_SAMPLE_FMT_S64:
                    case AVSampleFormat.AV_SAMPLE_FMT_S64P:
                        return 64;
                    default:
                        return 0;
                }
            }

            MediaInfo result;

            AVFormatContext* formatCtx = avformat_alloc_context();
            bool isOpen = false;
            try
            {
                int res = avformat_open_input(&formatCtx, filePath, null, null);
                if (res != 0)
                    return null;

                isOpen = true;

                if (avformat_find_stream_info(formatCtx, null) < 0)
                    return null;

                double durationSecs = formatCtx->duration > 0 ? formatCtx->duration / (double)AV_TIME_BASE : 0;

                long formatDurationTicks = formatCtx->duration > 0 ? formatCtx->duration * 10 : 0;

                string name = FFMPEGUtils.BytePtrToStringUTF8(formatCtx->iformat->name);
                string longName = FFMPEGUtils.BytePtrToStringUTF8(formatCtx->iformat->long_name);
                string extensions = FFMPEGUtils.BytePtrToStringUTF8(formatCtx->iformat->extensions);

                CodecDescription formatCodec;
                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(longName))
                    formatCodec = new CodecDescription(FourCC.FromString(name), longName);
                else
                    formatCodec = new CodecDescription(FourCC.FromString(extensions), extensions);

                result = new MediaInfo()
                {
                    Format = formatCodec,
                    Duration = TimeSpan.FromSeconds(durationSecs),
                };

                for (uint streamIndex = 0; streamIndex < formatCtx->nb_streams; streamIndex++)
                {
                    AVStream* st = formatCtx->streams[streamIndex];

                    double streamTimeBase = av_q2d(st->time_base) * 10000.0 * 1000.0;

                    long streamDurationTicks;
                    if (st->duration != AV_NOPTS_VALUE)
                        streamDurationTicks = (long)(st->duration * streamTimeBase);
                    else
                        streamDurationTicks = formatDurationTicks;

                    AVCodecID codecID = st->codecpar->codec_id;
                    string codecName = avcodec_get_name(codecID);
                    uint codecTag = st->codecpar->codec_tag;
                    AVMediaType codecType = st->codecpar->codec_type;

                    var metaData = new Dictionary<string, string>();

                    AVDictionaryEntry* b = null;
                    while (true)
                    {
                        b = av_dict_get(st->metadata, "", b, AV_DICT_IGNORE_SUFFIX);
                        if (b == null) break;
                        string key = FFMPEGUtils.BytePtrToStringUTF8(b->key);
                        string value = FFMPEGUtils.BytePtrToStringUTF8(b->value);
                        metaData.Add(key, value);
                    }

                    string language = null;
                    foreach (var key in metaData.Keys)
                    {
                        if ("language".Equals(key, StringComparison.InvariantCultureIgnoreCase) ||
                            "lang".Equals(key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            language = metaData[key];
                            break;
                        }
                    }

                    switch (codecType)
                    {
                        case AVMEDIA_TYPE_VIDEO:
                            {
                                CodecDescription codecDesc = new CodecDescription(FourCC.FromUInt(codecTag), codecName);
                                double fps = av_q2d(st->avg_frame_rate) > 0 ? av_q2d(st->avg_frame_rate) : av_q2d(st->r_frame_rate);
                                long frameDuration = fps > 0 ? (long)(10000000 / fps) : 0;
                                long frameCount = st->duration > 0 && frameDuration > 0 ? (int)(st->duration * streamTimeBase / frameDuration) : (int)(formatDurationTicks / frameDuration);
                                int width = st->codecpar->width;
                                int height = st->codecpar->height;
                                VideoInfo info = new VideoInfo()
                                {
                                    Name = $"Video[{st->index}]",
                                    Codec = codecDesc,
                                    Width = width,
                                    Height = height,
                                    FrameCount = frameCount,
                                    FrameRate = fps,
                                };
                                result.Video.Add(info);
                            }
                            break;
                        case AVMEDIA_TYPE_AUDIO:
                            {
                                int bitsPerSample = st->codecpar->bits_per_raw_sample > 0 ? st->codecpar->bits_per_raw_sample : GetBitsPerSample((AVSampleFormat)st->codecpar->format);
                                CodecDescription codecDesc = new CodecDescription(FourCC.FromUInt(codecTag), codecName);
                                AudioInfo info = new AudioInfo()
                                {
                                    Name = $"Audio[{st->index}]",
                                    Codec = codecDesc,
                                    Channels = st->codecpar->channels,
                                    SampleRate = st->codecpar->sample_rate,
                                    BitsPerSample = bitsPerSample,
                                    BitRate = st->codecpar->bit_rate,
                                    Lang = language,
                                };
                                result.Audio.Add(info);
                            }
                            break;
                        case AVMEDIA_TYPE_SUBTITLE:
                            {
                                SubtitleInfo info = new SubtitleInfo()
                                {
                                    Name = $"Subtitle[{st->index}]",
                                    Lang = language,
                                };
                                result.Subtitles.Add(info);
                            }
                            break;
                    }
                }
            }
            finally
            {
                if (formatCtx != null)
                {
                    if (isOpen)
                        avformat_close_input(&formatCtx);

                    avformat_free_context(formatCtx);
                }
            }




#if false
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
#endif

            return result;
        });
    }
}
