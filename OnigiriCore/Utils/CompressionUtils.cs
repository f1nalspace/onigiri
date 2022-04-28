using Finalspace.Onigiri.Crypto;
using System;
using System.IO;
using System.IO.Compression;

namespace Finalspace.Onigiri.Utils
{
    static class CompressionUtils
    {
        public static uint ComputeCRC(ReadOnlySpan<byte> data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            byte[] hash = CRC32.ComputeHash(data);
            if (hash.Length == 4)
            {
                uint result;
                if (BitConverter.IsLittleEndian)
                {
                    result =
                        ((uint)hash[0] << 0) |
                        ((uint)hash[1] << 8) |
                        ((uint)hash[2] << 16) |
                        ((uint)hash[3] << 24);
                }
                else
                {
                    result =
                        ((uint)hash[0] << 24) |
                        ((uint)hash[1] << 16) |
                        ((uint)hash[2] << 8) |
                        ((uint)hash[3] << 0);
                }
                return result;
            }
            return 0;
        }

        public static uint ComputeCRC(MemoryStream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            byte[] data = stream.ToArray();
            return ComputeCRC(data);
        }

        public static MemoryStream CompressGZip(MemoryStream uncompressedStream)
        {
            if (uncompressedStream == null)
                throw new ArgumentNullException(nameof(uncompressedStream));
            MemoryStream result = new MemoryStream();
            using (GZipStream compressor = new GZipStream(result, CompressionMode.Compress, true))
                uncompressedStream.CopyTo(compressor);
            result.Flush();
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }
        public static MemoryStream DecompressGZip(MemoryStream compressedStream)
        {
            if (compressedStream == null)
                throw new ArgumentNullException(nameof(compressedStream));
            MemoryStream result = new MemoryStream();
            using (GZipStream compressor = new GZipStream(compressedStream, CompressionMode.Decompress, true))
                compressor.CopyTo(result);
            result.Flush();
            return result;
        }

        public static MemoryStream CompressDeflate(MemoryStream uncompressedStream)
        {
            if (uncompressedStream == null)
                throw new ArgumentNullException(nameof(uncompressedStream));
            MemoryStream result = new MemoryStream();
            using (DeflateStream compressor = new DeflateStream(result, CompressionMode.Compress, true))
                uncompressedStream.CopyTo(compressor);
            result.Flush();
            result.Seek(0, SeekOrigin.Begin);
            return result;
        }
        public static MemoryStream DecompressDeflate(MemoryStream compressedStream)
        {
            if (compressedStream == null)
                throw new ArgumentNullException(nameof(compressedStream));
            MemoryStream result = new MemoryStream();
            using (DeflateStream compressor = new DeflateStream(result, CompressionMode.Decompress, true))
                compressedStream.CopyTo(result);
            result.Flush();
            return result;
        }
    }
}
