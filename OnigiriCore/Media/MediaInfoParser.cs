using Finalspace.Onigiri.Media.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Media
{
    public static class MediaInfoParser
    {
        private static readonly Dictionary<string, IMediaInfoParser> _extensionToParserMap = new Dictionary<string, IMediaInfoParser>()
        {
        };

        private static readonly FFMpegMediaInfoParser _defaultParser = new FFMpegMediaInfoParser();

        public static async Task<MediaInfo> Parse(FileInfo file)
        {
            if (file == null || !file.Exists)
                return null;
            string ext = file.Extension;
            string lowerExt = ext.ToLower();
            MediaInfo result = await _defaultParser.Parse(file.FullName);
            if (result == null)
            {
                if (_extensionToParserMap.TryGetValue(lowerExt, out IMediaInfoParser mediaInfoParser))
                    result = await mediaInfoParser.Parse(file.FullName);
            }
            return result;
        }

        public static Task<MediaInfo> Parse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            FileInfo file = new FileInfo(filePath);
            return Parse(file);
        }
    }
}
