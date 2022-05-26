﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Finalspace.Onigiri.Helper;
using Finalspace.Onigiri.Types;
using Finalspace.Onigiri.Utils;

namespace Finalspace.Onigiri.Media.Parsers
{
    class FFProbeMediaInfoParser : IMediaInfoParser
    {
        private static readonly Regex _rexBasePattern = new Regex("(\\d+)/(\\d+)", RegexOptions.Compiled);

        public async Task<MediaInfo> Parse(string filePath)
        {
            // ffprobe - v quiet - print_format json - show_format - show_streams - print_format json [FILENAME]

            string executable = "ffprobe.exe";
            string arguments = $"-v quiet -show_format -show_streams -print_format xml \"{filePath}\"";

            try
            {
#if false
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = executable;
                startInfo.Arguments = arguments;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.WorkingDirectory = Path.GetDirectoryName(filePath);
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                int exitCode = 0;
                StringBuilder output = new StringBuilder();
                using (Process process = new Process())
                {
                    process.OutputDataReceived += (s, e) => output.AppendLine(e.Data);
                    process.ErrorDataReceived += (s, e) => output.AppendLine(e.Data);
                    process.StartInfo = startInfo;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                    exitCode = process.ExitCode;
                }
#endif

                ProcessAsyncHelper.ProcessResult res = await ProcessAsyncHelper.ExecuteShellCommand(executable, arguments, 0);

                if (res.Completed && res.ExitCode == 0 && res.Output.Length > 0)
                {
                    MediaInfo result = new MediaInfo();

                    XmlDocument doc = new XmlDocument();

                    try
                    {
                        string xml = res.Output.ToString().Trim();
                        doc.LoadXml(xml);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }

                    XmlNode rootNode = doc.SelectSingleNode("ffprobe");
                    if (rootNode != null)
                    {
                        XmlNode formatNode = rootNode.SelectSingleNode("format");
                        if (formatNode != null)
                        {
                            int streamCount = formatNode.GetAttribute("nb_streams", 0);
                            string formatName = formatNode.GetAttribute("format_name", string.Empty);
                            string formatLongName = formatNode.GetAttribute("format_long_name", string.Empty);
                            double duration = formatNode.GetAttribute("duration", 0.0);

                            result.Format = FourCC.FromString(formatName);
                            result.Duration = TimeSpan.FromSeconds(duration);
                        }

                        XmlNodeList streamsNodeList = rootNode.SelectNodes("streams/stream");
                        if (streamsNodeList != null)
                        {
                            foreach (XmlNode streamNode in streamsNodeList)
                            {
                                int index = streamNode.GetAttribute("index", 0);
                                string codecType = streamNode.GetAttribute("codec_type", string.Empty);
                                string codecTagString = streamNode.GetAttribute("codec_tag_string", string.Empty);
                                string codecName = streamNode.GetAttribute("codec_name", string.Empty);
                                string codecLongName = streamNode.GetAttribute("codec_long_name", string.Empty);

                                switch (codecType)
                                {
                                    case "video":
                                        string frameRateText = streamNode.GetAttribute("avg_frame_rate", string.Empty);
                                        if (frameRateText.Length == 0 || frameRateText.EndsWith("/0"))
                                            frameRateText = streamNode.GetAttribute("r_frame_rate", string.Empty);

                                        double frameRate = 0.0;
                                        Match m = _rexBasePattern.Match(frameRateText);
                                        if (m.Success)
                                        {
                                            int value = int.Parse(m.Groups[1].Value);
                                            int divider = int.Parse(m.Groups[2].Value);
                                            if (divider != 0)
                                                frameRate = value / (double)divider;
                                        }

                                        FourCC codecId = FourCC.FromString(codecTagString);
                                        string codecDesc = codecLongName ?? codecName;
                                        int width = streamNode.GetAttribute("width", 0);
                                        int height = streamNode.GetAttribute("height", 0);
                                        int frameCount = streamNode.GetAttribute("nb_frames", 0);

                                        if (frameCount == 0 && frameRate > 0 && result.Duration.TotalSeconds > 0)
                                            frameCount = (int)(result.Duration.TotalSeconds * frameRate);

                                        result.Video.Add(new VideoInfo()
                                        {
                                            Codec = new CodecDescription(codecId, codecDesc),
                                            Width = width,
                                            Height = height,
                                            FrameCount = frameCount,
                                            FrameRate = frameRate,
                                        });
                                        break;

                                    case "audio":
                                        result.Audio.Add(new AudioInfo()
                                        {
                                            Codec = new CodecDescription(FourCC.FromString(codecName), codecLongName ?? codecName),
                                            Channels = streamNode.GetAttribute("channels", 0u),
                                            SampleRate = streamNode.GetAttribute("sample_rate", 0u),
                                            BitsPerSample = streamNode.GetAttribute("bits_per_sample", 0u),
                                            BitRate = streamNode.GetAttribute("bit_rate", 0u),
                                            FrameCount = streamNode.GetAttribute("nb_frames", 0u),
                                        });
                                        break;

                                    default:
                                        // @TODO(final): Subtitles!
                                        break;

                                }
                            }
                        }
                    }

                    return result;
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return null;
        }
    }
}
