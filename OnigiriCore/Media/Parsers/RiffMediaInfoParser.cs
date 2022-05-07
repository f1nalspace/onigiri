using Finalspace.Onigiri.Extensions;
using Finalspace.Onigiri.Types;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Finalspace.Onigiri.Media.Parsers
{
    public sealed class RiffMediaInfoParser : IMediaInfoParser
    {
        public static readonly FourCC RiffFormat = FourCC.FromString("RIFF");

        public static readonly FourCC AviFormat = FourCC.FromString("AVI ");

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct RiffHeader
        {
            public static readonly FourCC Magic = FourCC.FromString("RIFF");

            public FourCC magic;
            public UInt32 fileSize;
            public FourCC fileType;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct RiffChunkHeader
        {
            public FourCC type;
            public UInt32 size;
        }

        public MediaInfo Parse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                return null;
            MediaInfo result = new MediaInfo();
            using (FileStream stream = File.OpenRead(filePath))
            {
                // https://www.codeproject.com/Articles/10613/C-RIFF-Parser
                // http://www.jmcgowan.com/avitech.html

                RiffHeader mainHeader = stream.ReadStruct<RiffHeader>();
                if (mainHeader.magic != RiffHeader.Magic)
                    throw new FormatException($"Invalid RIFF magic, expect '{RiffHeader.Magic}' but got '{mainHeader.magic}'");

                if (mainHeader.fileType == AviFormat)
                {
                    result.Format = AviFormat; 
                    Debug.WriteLine($"File size: {mainHeader.fileSize}");
                    Debug.WriteLine($"File type: {mainHeader.fileType}");
                }
            }
            return result;
        }
    }
}
