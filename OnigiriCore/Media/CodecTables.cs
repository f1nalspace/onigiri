using Finalspace.Onigiri.Types;
using System.Collections.Generic;

namespace Finalspace.Onigiri.Media
{
    static class CodecTables
    {
        // https://www.originlab.com/doc/LabTalk/ref/FourCC-Table
        public static readonly CodecDescription VideoCodecUnknown = new CodecDescription(FourCC.Empty, "Uncompressed or unknown");
        public static readonly CodecDescription VideoCodecXvid = new CodecDescription(FourCC.FromString("xvid"), "XviD Codec");
        public static readonly CodecDescription VideoCodecDivX = new CodecDescription(FourCC.FromString("divx"), "DivX Codec");
        public static readonly CodecDescription VideoCodecMPEG4Generic = new CodecDescription(FourCC.FromString("mp4 "), "MPEG4");
        public static readonly CodecDescription VideoCodecMPEG4FFMpeg = new CodecDescription(FourCC.FromString("fmp4"), "MPEG4 (FFmpeg)");
        public static readonly CodecDescription VideoCodecMPEG4Msv2 = new CodecDescription(FourCC.FromString("mp42"), "MSMPEG4v2, a MPEG-4 variation");
        public static readonly CodecDescription VideoCodecMPEG4Msv3 = new CodecDescription(FourCC.FromString("mp43"), "MSMPEG4v3, a MPEG-4 variation");
        public static readonly CodecDescription VideoCodecWMV1 = new CodecDescription(FourCC.FromString("wmv1"), "WMV1");
        public static readonly CodecDescription VideoCodecWMV2 = new CodecDescription(FourCC.FromString("wmv2"), "WMV2");

        public static readonly Dictionary<FourCC, CodecDescription> IdToVideoCodecMap = new Dictionary<FourCC, CodecDescription>()
        {
            { FourCC.Empty, VideoCodecUnknown },

            { FourCC.FromString("xvid"), VideoCodecXvid },
            { FourCC.FromString("XVID"), VideoCodecXvid },
            { FourCC.FromString("XviD"), VideoCodecXvid },

            { FourCC.FromString("divx"), VideoCodecDivX },
            { FourCC.FromString("DIVX"), VideoCodecDivX },
            { FourCC.FromString("DivX"), VideoCodecDivX },

            { FourCC.FromString("mp4 "), VideoCodecMPEG4Generic },
            { FourCC.FromString("MP4 "), VideoCodecMPEG4Generic },
            { FourCC.FromString("fmp4"), VideoCodecMPEG4FFMpeg },
            { FourCC.FromString("FMP4"), VideoCodecMPEG4FFMpeg },
            { FourCC.FromString("mp42"), VideoCodecMPEG4Msv2 },
            { FourCC.FromString("MP42"), VideoCodecMPEG4Msv2 },
            { FourCC.FromString("mp43"), VideoCodecMPEG4Msv3 },
            { FourCC.FromString("MP43"), VideoCodecMPEG4Msv3 },

            { FourCC.FromString("wmv1"), VideoCodecWMV1 },
            { FourCC.FromString("WMV1"), VideoCodecWMV1 },
            { FourCC.FromString("wmv2"), VideoCodecWMV2 },
            { FourCC.FromString("WMV2"), VideoCodecWMV2 },
        };

        // https://docs.microsoft.com/en-us/windows/win32/directshow/audio-subtypes
        public static readonly Dictionary<uint, CodecDescription> FormatTagToAudioCodecMap = new Dictionary<uint, CodecDescription>()
        {
            //
            // Uncompressed
            //
            {0x0001, new CodecDescription(FourCC.FromString("PCM "), "Uncompressed PCM")}, // WAVE_FORMAT_PCM
            {0x0003, new CodecDescription(FourCC.FromString("IFLT"), "Uncompressed IEEE Float")}, // // WAVE_FORMAT_IEEE_FLOAT

            //
            // MPEG-4 and AAC Audio Types
            //
            {0x1600, new CodecDescription(FourCC.FromString("AAC "), "Advanced Audio Coding (AAC)")}, // WAVE_FORMAT_MPEG_ADTS_AAC
            {0x1610, new CodecDescription(FourCC.FromString("HAAC"), "High-Efficiency Advanced Audio Coding (HE-AAC)")}, // WAVE_FORMAT_MPEG_HEAAC
            {0x1602, new CodecDescription(FourCC.FromString("LOAS"), "MPEG-4 audio LOAS")}, // WAVE_FORMAT_MPEG_LOAS
            {0x00FF, new CodecDescription(FourCC.FromString("RAAC"), "Raw AAC")}, // WAVE_FORMAT_RAW_AAC1

            //
            // Dolby Audio Types
            //
            {0x0092, new CodecDescription(FourCC.FromString("AC3 "), "Dolby AC-3 over S/PDIF.")}, // WAVE_FORMAT_DOLBY_AC3_SPDIF
            {0x2000, new CodecDescription(FourCC.FromString("AC3 "), "DVM AC-3 codec")}, // WAVE_FORMAT_DVM
            {0x0240, new CodecDescription(FourCC.FromString("AC3 "), "AC-3 over S/PDIF")}, // WAVE_FORMAT_RAW_SPORT
            {0x0241, new CodecDescription(FourCC.FromString("AC3 "), "AC-3 over S/PDIF")}, // WAVE_FORMAT_ESST_AC3

            // Miscellaneous Audio Types
            {0x0009, new CodecDescription(FourCC.FromString("ADRM"), "Audio with digital rights management (DRM) protection")}, // WAVE_FORMAT_DRM
            {0x2001, new CodecDescription(FourCC.FromString("DTS2"), "Digital Theater Systems (DTS) audio")}, // WAVE_FORMAT_DTS2
            {0x0050, new CodecDescription(FourCC.FromString("MPG1"), "MPEG-1 audio")}, // WAVE_FORMAT_MPEG
            {0x0055, new CodecDescription(FourCC.FromString("MP3 "), "MPEG-2 Layer-3")}, // MP3
        };
    }
}
