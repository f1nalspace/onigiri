using Finalspace.Onigiri.Types;
using Finalspace.Onigiri.Utils;
using log4net;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.AniDB
{
    interface IHttpApi
    {
        Task<TextContent> RequestAnimeAsync(ulong aid);
        Task DownloadTitlesDumpAsync(string targetFilePath);
        Task DownloadPictureAsync(string picture, string targetFilePath);
    }

    class HttpApi : IHttpApi
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static string ApiURL = "http://api.anidb.net:9001/httpapi?client={{clientName}}&clientver={{clientVer}}&protover={{protoVer}}&request=anime&aid={{aid}}";
        private static string TitlesDumpURL = "https://anidb.net/api/animetitles.xml.gz";
        private static string ImageServerURL = "http://img7.anidb.net/pics/anime/";
        public static string TitlesDumpFilename = "animetitles.xml.gz";

        private static string ClientName = "onigiri";
        private static string ClientVer = "1";
        private static string ProtoVer = "1";
        private static int DefaultDelay = 3000;

        public async Task<TextContent> RequestAnimeAsync(ulong aid)
        {
            string url = ApiURL;
            url = url.Replace("{{clientName}}", ClientName);
            url = url.Replace("{{clientVer}}", ClientVer);
            url = url.Replace("{{protoVer}}", ProtoVer);
            url = url.Replace("{{aid}}", aid.ToString());
            try
            {
                await Task.Delay(DefaultDelay);
                TextContent result = await HttpUtils.DownloadTextAsync(url);
                return result;
            }
            catch (Exception e)
            {
                log.Error($"Failed requesting anime details from '{url}'!", e);
            }
            return null;
        }

        public async Task DownloadTitlesDumpAsync(string targetFilePath)
        {
            try
            {
                await Task.Delay(DefaultDelay);
                await HttpUtils.DownloadFileAsync(TitlesDumpURL, targetFilePath);
            }
            catch (Exception e)
            {
                log.Error($"Failed downloading titles dump from '{TitlesDumpURL}' to '{targetFilePath}'!", e);
            }
        }

        public async Task DownloadPictureAsync(string picture, string targetFilePath)
        {
            string imageExt = Path.GetExtension(picture);
            string sourcePictureUrl = ImageServerURL + picture;
            targetFilePath = Path.ChangeExtension(targetFilePath, imageExt);
            try
            {
                await Task.Delay(DefaultDelay);
                await HttpUtils.DownloadFileAsync(sourcePictureUrl, targetFilePath);
            } catch (Exception e)
            {
                log.Error($"Failed downloading picture from '{sourcePictureUrl}' to '{targetFilePath}'!", e);
            }
        }
    }
}
