using Finalspace.Onigiri.Types;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace Finalspace.Onigiri.Utils
{
    static class HttpUtils
    {
        

        public static void DownloadFile(string url, string filePath)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Onigiri");
                byte[] data = client.DownloadData(url);
                using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    stream.Write(data, 0, data.Length);
                }
            }
        }

        public static TextContent DownloadText(string url)
        {
            TextContent result = new TextContent();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("user-agent", "Onigiri");
                byte[] data = client.DownloadData(url);
                string encoding = client.ResponseHeaders[HttpResponseHeader.ContentEncoding];
                if ("gzip".Equals(encoding, StringComparison.InvariantCultureIgnoreCase))
                {
                    using (GZipStream stream = new GZipStream(new MemoryStream(data), CompressionMode.Decompress))
                    {
                        const int size = 4096;
                        byte[] buffer = new byte[size];
                        byte[] decoded = null;
                        using (MemoryStream memory = new MemoryStream())
                        {
                            int count = 0;
                            do
                            {
                                count = stream.Read(buffer, 0, size);
                                if (count > 0)
                                {
                                    memory.Write(buffer, 0, count);
                                }
                            }
                            while (count > 0);
                            decoded = memory.ToArray();
                        }
                        data = decoded;
                    }
                }
                result.Encoding = Encoding.UTF8;
                result.Text = Encoding.UTF8.GetString(data);
            }
            return result;
        }

    }
}
