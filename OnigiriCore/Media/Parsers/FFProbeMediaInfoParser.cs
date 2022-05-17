using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Finalspace.Onigiri.Types;
using Finalspace.Onigiri.Utils;

namespace Finalspace.Onigiri.Media.Parsers
{
    class FFProbeMediaInfoParser : IMediaInfoParser
    {
        private static readonly Regex _rexBasePattern = new Regex("(\\d+)/(\\d+)", RegexOptions.Compiled);

        public MediaInfo Parse(string filePath)
        {
            // ffprobe - v quiet - print_format json - show_format - show_streams - print_format json [FILENAME]

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "ffprobe.exe";
                startInfo.Arguments = $"-v quiet -show_format -show_streams -print_format xml \"{filePath}\"";
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

                if (exitCode == 0 && output.Length > 0)
                {
                    MediaInfo result = new MediaInfo();

                    string xml = output.ToString().Trim();

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);

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
                                        string avgFrameRate = streamNode.GetAttribute("avg_frame_rate", string.Empty);

                                        double frameRate = 0.0;
                                        Match m = _rexBasePattern.Match(avgFrameRate);
                                        if (m.Success)
                                        {
                                            int value = int.Parse(m.Groups[1].Value);
                                            int divider = int.Parse(m.Groups[2].Value);
                                            frameRate = value / (double)divider;
                                        }

                                        result.Video.Add(new VideoInfo()
                                        {
                                            Codec = new CodecDescription(FourCC.FromString(codecTagString), codecLongName ?? codecName),
                                            Width = streamNode.GetAttribute("width", 0),
                                            Height = streamNode.GetAttribute("height", 0),
                                            FrameCount = streamNode.GetAttribute("nb_frames", 0),
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
