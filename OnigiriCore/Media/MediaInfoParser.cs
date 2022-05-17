using Finalspace.Onigiri.Media.Parsers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Finalspace.Onigiri.Media
{
    public static class MediaInfoParser
    {
        //public static readonly FourCC MatroskaFormat = FourCC.FromString("MKV_");
        //public static readonly FourCC OggFormat = FourCC.FromString("OGM_");
        //public static readonly FourCC MP4Format = FourCC.FromString("MP4_");
        //public static readonly FourCC MpegFormat = FourCC.FromString("MPEG");

        private static readonly Dictionary<string, IMediaInfoParser> _extensionToParserMap = new Dictionary<string, IMediaInfoParser>()
        {
            {".avi",  new RiffMediaInfoParser() },
        };

        private static readonly FFProbeMediaInfoParser _ffProbeParser = new FFProbeMediaInfoParser();

        public static MediaInfo Parse(FileInfo file)
        {
            if (file == null || !file.Exists)
                return null;
            string ext = file.Extension;
            string lowerExt = ext.ToLower();
            MediaInfo result = _ffProbeParser.Parse(file.FullName);
            if (result == null)
            {
                if (_extensionToParserMap.TryGetValue(lowerExt, out IMediaInfoParser mediaInfoParser))
                    result = mediaInfoParser.Parse(file.FullName);
            }
            return result;
        }

        public static MediaInfo Parse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            FileInfo file = new FileInfo(filePath);
            return Parse(file);
        }
    }
}
