using System.IO;
using System.IO.Compression;

namespace Finalspace.Onigiri.Utils
{
    static class FileUtils
    {
        public static byte[] LoadFileData(string filePath)
        {
            FileInfo f = new FileInfo(filePath);
            if (f.Exists)
            {
                long size = f.Length;
                byte[] result = new byte[size];
                using (Stream inputStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    inputStream.Read(result, 0, (int)size);
                }
                return (result);
            }
            return null;
        }

        public static Stream LoadFromData(byte[] data)
        {
            Stream result = new MemoryStream(data);
            return result;
        }

        public static void DecompressFile(string compressedFilePath, string outputFilePath)
        {
            using (Stream inputStream = new FileStream(compressedFilePath, FileMode.Open, FileAccess.Read))
            {
                using (GZipStream stream = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    const int size = 4096;
                    byte[] buffer = new byte[size];
                    using (Stream outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if (count > 0)
                            {
                                outputStream.Write(buffer, 0, count);
                            }
                        }
                        while (count > 0);
                    }
                }
            }
        }
    }
}
