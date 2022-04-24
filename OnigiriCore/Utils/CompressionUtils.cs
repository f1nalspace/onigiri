using System;
using System.IO;
using System.IO.Compression;

namespace Finalspace.Onigiri.Utils
{
    static class CompressionUtils
    {
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
