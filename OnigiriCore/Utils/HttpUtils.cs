using Finalspace.Onigiri.Types;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.Utils
{
    static class HttpUtils
    {
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";

        public static async Task DownloadFileAsync(string url, string targetFilePath, CancellationToken cancellationToken = default)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            using (HttpClient hclient = new HttpClient())
            {
                hclient.DefaultRequestHeaders.Add("user-agent", UserAgent);

                using (HttpResponseMessage response = await hclient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken))
                {
                    response.EnsureSuccessStatusCode();

                    using HttpContent content = response.Content;

                    using (Stream outputStream = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write))
                    {
                        using Stream sourceStream = await content.ReadAsStreamAsync(cancellationToken);
                        await sourceStream.CopyToAsync(outputStream, cancellationToken);
                        outputStream.Flush();
                    }
                }
            }
        }

        public static async Task<TextContent> DownloadTextAsync(string url, CancellationToken cancellationToken = default)
        {
            TextContent result = new TextContent();

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            using (HttpClient hclient = new HttpClient())
            {
                hclient.DefaultRequestHeaders.Add("user-agent", UserAgent);

                using (HttpResponseMessage response = await hclient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken))
                {
                    response.EnsureSuccessStatusCode();

                    using (HttpContent content = response.Content)
                    {

                        string[] encodings = content.Headers.ContentEncoding.Select(s => s.ToLower()).ToArray();

                        byte[] data = await content.ReadAsByteArrayAsync(cancellationToken);

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
                                        count = await stream.ReadAsync(buffer, 0, size, cancellationToken);
                                        if (count > 0)
                                        {
                                            await memory.WriteAsync(buffer, 0, count, cancellationToken);
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
