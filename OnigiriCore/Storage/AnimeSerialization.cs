using Finalspace.Onigiri.Models;
using log4net;
using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Storage
{
    static class AnimeSerialization
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static Anime DeserializeAnime(ReadOnlySpan<byte> data, ulong aid)
        {
            Anime anime = null;
            try
            {
                // TODO(final): Replace XmlSerializer with manual serialization
                XmlSerializer serializer = new XmlSerializer(typeof(Anime));
                using (MemoryStream stream = new MemoryStream(data.ToArray()))
                {
                    anime = (Anime)serializer.Deserialize(stream);
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed to deserialize anime data from data with lengt '{data.Length}' and aid '{aid}'!", e);
                return null;
            }
            return anime;
        }

        public static MemoryStream SerializeAnime(Anime anime)
        {
            MemoryStream result = new MemoryStream();
            try
            {
                // TODO(final): Replace XmlSerializer with manual serialization
                XmlSerializer serializer = new XmlSerializer(typeof(Anime));
                serializer.Serialize(result, anime);
                result.Flush();
                result.Seek(0, SeekOrigin.Begin);
            }
            catch (Exception e)
            {
                log.Error($"Failed to serialize anime '{anime}' to xml!", e);
                result.Dispose();
                return null;
            }
            return result;
        }
    }
}
