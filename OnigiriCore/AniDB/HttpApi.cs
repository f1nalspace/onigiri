using Finalspace.Onigiri.Types;
using Finalspace.Onigiri.Utils;
using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Finalspace.Onigiri.AniDB
{
    public static class HttpApi
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static string ApiURL = "http://api.anidb.net:9001/httpapi?client={{clientName}}&clientver={{clientVer}}&protover={{protoVer}}&request=anime&aid={{aid}}";
        private static string TitlesDumpURL = "https://anidb.net/api/animetitles.xml.gz";
        private static string ImageServerURL = "http://img7.anidb.net/pics/anime/";
        public static string TitlesDumpFilename = "animetitles.xml.gz";

        private static string ClientName = "onigiri";
        private static string ClientVer = "1";
        private static string ProtoVer = "1";
        private static int DefaultDelay = 1500;

        public static async Task<TextContent> RequestAnimeAsync(ulong aid)
        {
            Thread.Sleep(DefaultDelay);
            string url = ApiURL;
            url = url.Replace("{{clientName}}", ClientName);
            url = url.Replace("{{clientVer}}", ClientVer);
            url = url.Replace("{{protoVer}}", ProtoVer);
            url = url.Replace("{{aid}}", aid.ToString());
            try
            {
                TextContent result = await HttpUtils.DownloadTextAsync(url);
                return result;
            }
            catch (Exception e)
            {
                log.Error($"Failed requesting anime details from '{url}'!", e);
            }
            return null;
        }

        public static async Task DownloadTitlesDumpAsync(string targetFilePath)
        {
            Thread.Sleep(DefaultDelay);
            try
            {
                await HttpUtils.DownloadFileAsync(TitlesDumpURL, targetFilePath);
            }
            catch (Exception e)
            {
                log.Error($"Failed downloading titles dump from '{TitlesDumpURL}' to '{targetFilePath}'!", e);
            }
        }

        public static async Task DownloadPictureAsync(string picture, string targetFilePath)
        {
            string imageExt = Path.GetExtension(picture);
            string sourcePictureUrl = ImageServerURL + picture;
            targetFilePath = Path.ChangeExtension(targetFilePath, imageExt);
            Thread.Sleep(DefaultDelay);
            try
            {
                await HttpUtils.DownloadFileAsync(sourcePictureUrl, targetFilePath);
            } catch (Exception e)
            {
                log.Error($"Failed downloading picture from '{sourcePictureUrl}' to '{targetFilePath}'!", e);
            }
        }
    }
}
