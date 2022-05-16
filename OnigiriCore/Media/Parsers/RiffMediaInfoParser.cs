using Finalspace.Onigiri.Extensions;
using Finalspace.Onigiri.Types;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Finalspace.Onigiri.Media.Parsers
{
    public sealed class RiffMediaInfoParser : IMediaInfoParser
    {
        public static readonly FourCC AviFormat = FourCC.FromString("AVI ");

        public static readonly FourCC RiffType = FourCC.FromString("RIFF");

        public static readonly FourCC AviFormType = FourCC.FromString("AVI ");

        public static readonly FourCC ListChunkType = FourCC.FromString("LIST");

        public static readonly FourCC HeaderListType = FourCC.FromString("hdrl");

        public static readonly FourCC AviHeaderChunkType = FourCC.FromString("avih");

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct RiffHeader
        {
            public FourCC magic;
            public UInt32 fileSize;
            public FourCC formType;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct RiffChunkHeader
        {
            public FourCC type;
            public UInt32 size;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        unsafe struct AviHeader
        {
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

        private bool ParseAviHeader(StreamData data, long headerSize, ref MediaInfo mediaInfo)
        {
            long expectedSize = 56;
            if (headerSize != expectedSize)
                throw new FormatException($"Invalid AVI Header size, expect '{expectedSize}' but got '{headerSize}'");

            data.Seek(expectedSize, SeekOrigin.Current);
            return true;
        }

        private bool ParseChunk(StreamData data, ref MediaInfo mediaInfo)
        {
            using BinaryReader binaryReader = new BinaryReader(data, Encoding.ASCII, true);

            FourCC ident = data.ReadStruct<FourCC>();
            if (ident.IsEmpty)
                return false;

            UInt32 chunkSize = binaryReader.ReadUInt32();
            if (chunkSize == 0)
                return false;

            if (AviHeaderChunkType.Equals(ident))
            {
                if (!ParseAviHeader(data, chunkSize, ref mediaInfo))
                    return false;
                while (data.CanRead)
                {
                    if (!ParseChunk(data, ref mediaInfo))
                        return false;
                }
                return true;
            }
            else if (ListChunkType.Equals(ident))
            {
                if (!ParseList(data, ref mediaInfo))
                    return false;
            }
            else
                data.Seek(chunkSize, SeekOrigin.Current);

            return true;
        }

        private bool ParseList(StreamData data, ref MediaInfo mediaInfo)
        {
            using BinaryReader binaryReader = new BinaryReader(data, Encoding.ASCII, true);

            FourCC ident = data.ReadStruct<FourCC>();
            if (ident.IsEmpty)
                return false;

            UInt32 listSize = binaryReader.ReadUInt32();
            if (listSize == 0)
                return false;

            if (HeaderListType.Equals(ident))
            {
                if (ParseChunk(data, ref mediaInfo))
                    return true;
            }

            return false;
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

                while (streamData.CanRead)
                {
                    RiffHeader mainHeader = stream.ReadStruct<RiffHeader>();
                    if (mainHeader.magic != RiffType)
                        throw new FormatException($"Invalid RIFF magic, expect '{RiffHeader.Magic}' but got '{mainHeader.magic}'");

                    Debug.WriteLine($"File size: {mainHeader.fileSize}");
                    Debug.WriteLine($"Form type: {mainHeader.formType}");

                    if (mainHeader.formType == AviFormType)
                    {
                        result.Format = AviFormat;

                        while (stream.CanRead)
                        {
                            if (!ParseChunk(streamData, ref result))
                                break;
                        }
                    }
                }
            }
            return result;
        }
    }
}
