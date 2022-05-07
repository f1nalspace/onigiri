using Finalspace.Onigiri.Types;
using System;

namespace Finalspace.Onigiri.Media
{
    public struct AudioInfo
    {
        public string Lang { get; }
        public uint Channels { get; }
        public uint SampleRate { get; }
        public Type SampleType { get; }
        public FourCC Codec { get; }

        public AudioInfo(string language, uint channels, uint sampleRate, Type sampleType, FourCC codec)
        {
            Lang = language;
            Channels = channels;
            SampleRate = sampleRate;
            Codec = codec;
            SampleType = sampleType;
        }
    }
}
