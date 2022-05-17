using Finalspace.Onigiri.Extensions;
using Finalspace.Onigiri.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Finalspace.Onigiri.Media.Parsers
{
    public sealed class RiffMediaInfoParser : IMediaInfoParser
    {
        public static readonly FourCC AviFormat = FourCC.FromString("AVI ");

        public static readonly FourCC RiffType = FourCC.FromString("RIFF");
        public static readonly FourCC ListType = FourCC.FromString("LIST");
        public static readonly FourCC JunkType = FourCC.FromString("JUNK");
        public static readonly FourCC Idx1Type = FourCC.FromString("idx1");

        public static readonly FourCC AviType = FourCC.FromString("AVI ");

        public static readonly FourCC HeaderChunkType = FourCC.FromString("hdrl");
        public static readonly FourCC AviHeaderType = FourCC.FromString("avih");
        public static readonly FourCC StreamType = FourCC.FromString("strl");
        public static readonly FourCC StreamHeaderType = FourCC.FromString("strh");
        public static readonly FourCC StreamFormatType = FourCC.FromString("strf");

        public static readonly FourCC StreamVideoType = FourCC.FromString("vids");
        public static readonly FourCC StreamAudioType = FourCC.FromString("auds");

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct RiffChunk
        {
            public FourCC chunkType;
            public UInt32 chunkSize;
            // data (size)
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct RiffList
        {
            public FourCC listType; // Can be either RIFF or LIST
            public UInt32 listSize;
            public FourCC chunkType;
            // data (size - 4)
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct AviHeader
        {
            public static int Size => Marshal.SizeOf(typeof(AviHeader));

            public UInt32 dwMicroSecPerFrame;
            public UInt32 dwMaxBytesPerSec;
            public UInt32 dwPaddingGranularity;

            public UInt32 dwFlags;
            public UInt32 dwTotalFrames;
            public UInt32 dwInitialFrames;
            public UInt32 dwStreams;
            public UInt32 dwSuggestedBufferSize;

            public UInt32 dwWidth;
            public UInt32 dwHeight;

            public fixed UInt32 dwReserved[4];
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct AviRect
        {
            public short left;
            public short top;
            public short right;
            public short bottom;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct AviStreamHeader
        {
            public static int Size => Marshal.SizeOf(typeof(AviStreamHeader));

            public FourCC fccType;
            public FourCC fccHandler;
            public uint dwFlags;
            public ushort wPriority;
            public ushort wLanguage;
            public uint dwInitialFrames;
            public uint dwScale;
            public uint dwRate;
            public uint dwStart;
            public uint dwLength;
            public uint dwSuggestedBufferSize;
            public uint dwQuality;
            public uint dwSampleSize;
            public AviRect rcFrame;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WaveFormatEx
        {
            public static int Size => Marshal.SizeOf(typeof(WaveFormatEx));

            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct WaveFormatExtensible
        {
            [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 2)]
            public struct SamplesUnion
            {
                [FieldOffset(0)]
                public ushort wValidBitsPerSample;
                [FieldOffset(0)]
                public ushort wSamplesPerBlock;
                [FieldOffset(0)]
                public ushort wReserved;
            }

            public static int Size => Marshal.SizeOf(typeof(WaveFormatExtensible));

            public WaveFormatEx Format;
            public SamplesUnion Samples;
            public uint dwChannelMask;
            public Guid SubFormat;
        }

        class StreamData : Stream
        {
            public Stream Instance { get; }
            public byte[] Data { get; }

            public override bool CanRead => Instance.CanRead;
            public override bool CanSeek => Instance.CanSeek;
            public override bool CanWrite => Instance.CanWrite;
            public override long Length => Instance.Length;
            public override long Position { get => Instance.Position; set => Instance.Position = value; }

            public bool IsEOF => !(CanRead && Position < Length);

            public StreamData(Stream instance, byte[] data)
            {
                Instance = instance ?? throw new ArgumentNullException(nameof(instance));
                Data = data ?? throw new ArgumentNullException(nameof(data));
            }

            public override void Flush() => Instance.Flush();
            public override int Read(byte[] buffer, int offset, int count) => Instance.Read(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => Instance.Seek(offset, origin);
            public override void SetLength(long value) => Instance.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => Instance.Write(buffer, offset, count);
        }

        private static bool ParseAviHeader(StreamData streamData, long dataSize, ref MediaInfo result)
        {
            if (AviHeader.Size != dataSize)
                throw new FormatException($"Wrong AVI header size, expect '{AviHeader.Size}' but got '{dataSize}'!");

            AviHeader aviHeader = streamData.ReadStruct<AviHeader>();

            int frameCount = (int)aviHeader.dwTotalFrames;
            double frameRate = 1000.0 / ((double)aviHeader.dwMicroSecPerFrame / 1000.0);
            CodecDescription videoCodec = CodecDescription.Empty;

            VideoInfo videoInfo = new VideoInfo((int)aviHeader.dwWidth, (int)aviHeader.dwHeight, frameCount, frameRate, videoCodec);

            result.Video = videoInfo;

            return true;
        }

        private static bool ParseStream(StreamData streamData, long dataSize, ref MediaInfo result)
        {
            long endList = streamData.Position + dataSize;

            FourCC currentStreamType = FourCC.Empty;

            AudioInfo currentAudioInfo = null;

            while (!streamData.IsEOF && streamData.Position < endList)
            {
                RiffChunk chunk = streamData.ReadStruct<RiffChunk>();

                if (StreamHeaderType.Equals(chunk.chunkType))
                {
                    if (AviStreamHeader.Size != chunk.chunkSize)
                        throw new FormatException($"Wrong AVI header size, expect '{AviStreamHeader.Size}' but got '{chunk.chunkSize}'!");

                    AviStreamHeader streamHeader = streamData.ReadStruct<AviStreamHeader>();

                    currentStreamType = streamHeader.fccType;

                    if (StreamVideoType.Equals(streamHeader.fccType))
                    {
                        if (result.Video != null)
                        {
                            if (CodecTables.IdToVideoCodecMap.TryGetValue(streamHeader.fccHandler, out CodecDescription codecDescription))
                                result.Video.Codec = codecDescription;
                            else
                                result.Video.Codec = new CodecDescription(streamHeader.fccHandler);
                            result.Video.FrameRate = streamHeader.dwRate / (double)streamHeader.dwScale;
                            result.Video.FrameCount = (int)streamHeader.dwLength;
                        }
                    }
                    else if (StreamAudioType.Equals(streamHeader.fccType))
                    {
                        uint sampleRate = streamHeader.dwScale > 0 ? (uint)(streamHeader.dwRate / (double)streamHeader.dwScale) : streamHeader.dwRate;
                        uint bytesPerSample = streamHeader.dwSampleSize;
                        AudioInfo audioInfo = currentAudioInfo = new AudioInfo()
                        {
                            SampleRate = sampleRate,
                            BytesPerSample = bytesPerSample,
                        };
                        result.Audio.Add(audioInfo);
                    }
                    else
                        throw new NotImplementedException($"FCC Type '{streamHeader.fccType}' not implemented yet");
                }
                else if (StreamFormatType.Equals(chunk.chunkType))
                {
                    if (StreamVideoType.Equals(currentStreamType))
                    {
                        // BITMAPINFOHEADER
                    }
                    else if (StreamAudioType.Equals(currentStreamType))
                    {
                        // WAVEFORMAT / WAVEFORMATEX / WAVEFORMATEXTENSIBLE
                        if (currentAudioInfo != null)
                        {
                            if (chunk.chunkSize < WaveFormatEx.Size)
                                throw new FormatException($"Wrong Wave Format header size, expect at least '{WaveFormatEx.Size}' but got '{chunk.chunkSize}'!");

                            long position = streamData.Position;

                            WaveFormatEx waveFormatEx = streamData.ReadStruct<WaveFormatEx>();
                            currentAudioInfo.Channels = waveFormatEx.nChannels;
                            currentAudioInfo.SampleRate = waveFormatEx.nSamplesPerSec;
                            currentAudioInfo.BytesPerSample = (uint)(waveFormatEx.wBitsPerSample / 8);
                            if (CodecTables.FormatTagToAudioCodecMap.TryGetValue(waveFormatEx.wFormatTag, out CodecDescription codecDesc))
                                currentAudioInfo.Codec = codecDesc;
                            else
                                currentAudioInfo.Codec = CodecDescription.Empty;

                            // Seek back to the start position before we read the WaveFormatEx structure
                            uint remaining = chunk.chunkSize - (uint)WaveFormatEx.Size;
                            streamData.Seek(remaining, SeekOrigin.Current);
                            continue;
                        }
                    }

                    streamData.Seek(chunk.chunkSize, SeekOrigin.Current);
                }
                else
                    streamData.Seek(chunk.chunkSize, SeekOrigin.Current);
            }

            return true;
        }

        private static bool ParseHeaderList(StreamData streamData, long dataSize, ref MediaInfo result)
        {
            long endList = streamData.Position + dataSize;
            while (!streamData.IsEOF && streamData.Position < endList)
            {
                RiffChunk chunk = streamData.ReadStruct<RiffChunk>();

                long seekSize = chunk.chunkSize;
                FourCC chunkListType = FourCC.Empty;
                if (ListType.Equals(chunk.chunkType))
                {
                    chunkListType = streamData.ReadStruct<FourCC>();
                    seekSize -= 4;
                }

                if (AviHeaderType.Equals(chunk.chunkType))
                {
                    if (!ParseAviHeader(streamData, chunk.chunkSize, ref result))
                        return false;
                }
                else if (StreamType.Equals(chunkListType))
                {
                    Debug.Assert(ListType.Equals(chunk.chunkType));
                    if (!ParseStream(streamData, chunk.chunkSize - 4, ref result))
                        return false;
                }
                else
                    streamData.Seek(seekSize, SeekOrigin.Current);
            }

            return true;
        }

        private static bool ParseAviList(StreamData streamData, long dataSize, ref MediaInfo result)
        {
            long endList = streamData.Position + dataSize;
            while (!streamData.IsEOF && streamData.Position < endList)
            {
                RiffList listHeader = streamData.ReadStruct<RiffList>();
                if (HeaderChunkType.Equals(listHeader.chunkType))
                {
                    if (!ParseHeaderList(streamData, listHeader.listSize - 4, ref result))
                        return false;
                }
                else
                    streamData.Seek(listHeader.listSize - 4, SeekOrigin.Current);
            }
            return true;
        }

        public MediaInfo Parse(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                return null;

            MediaInfo result = new MediaInfo();

            byte[] data = File.ReadAllBytes(filePath);

            // https://cdn.hackaday.io/files/274271173436768/avi.pdf
            // https://www.codeproject.com/Articles/10613/C-RIFF-Parser
            // http://www.jmcgowan.com/avitech.html

            using (FileStream stream = File.OpenRead(filePath))
            {
                StreamData streamData = new StreamData(stream, data);

                RiffList mainHeader = stream.ReadStruct<RiffList>();
                if (!RiffType.Equals(mainHeader.listType))
                    throw new FormatException($"Invalid RIFF format, expect '{RiffType}' but got '{mainHeader.listType}'!");

                if (!AviType.Equals(mainHeader.chunkType))
                    throw new FormatException($"Unsupported RIFF chunk type, expect '{AviType}' but got '{mainHeader.chunkType}'!");

                result.Format = AviFormat;

                ParseAviList(streamData, mainHeader.listSize - 4, ref result);
            }

            return result;
        }
    }
}
