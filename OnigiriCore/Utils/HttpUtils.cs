using Finalspace.Onigiri.Types;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Utils
{
    static class HttpUtils
    {
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";

        public static void DownloadFile(string url, string filePath)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            using (HttpClient hclient = new HttpClient())
            {
                hclient.DefaultRequestHeaders.Add("user-agent", UserAgent);

                using (HttpResponseMessage response = hclient.Send(new HttpRequestMessage(HttpMethod.Get, url)))
                {
                    response.EnsureSuccessStatusCode();

                    using HttpContent content = response.Content;

                    using (Stream outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        using Stream sourceStream = content.ReadAsStream();
                        sourceStream.CopyTo(outputStream);
                        outputStream.Flush();
                    }
                }
            }
        }

        public static TextContent DownloadText(string url)
        {
            TextContent result = new TextContent();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            using (HttpClient hclient = new HttpClient())
            {
                hclient.DefaultRequestHeaders.Add("user-agent", UserAgent);

                using (HttpResponseMessage response = hclient.Send(new HttpRequestMessage(HttpMethod.Get, url)))
                {
                    response.EnsureSuccessStatusCode();

                    using (HttpContent content = response.Content)
                    {

                        string[] encodings = content.Headers.ContentEncoding.Select(s => s.ToLower()).ToArray();

                        Task<byte[]> task = content.ReadAsByteArrayAsync();
                        task.Wait();

                        byte[] data = task.Result;

                        bool isGZip = encodings.Contains("gzip");
                        if (isGZip)
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
                }
            }

            return result;
        }

    }
}
