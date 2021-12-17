using Finalspace.Onigiri.Extensions;
using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

namespace Finalspace.Onigiri.Persistence
{
    public class PersistentAnime
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ulong Aid { get; }

        public byte[] Details { get; }

        public byte[] Picture { get; }

        public PersistentAnime(ulong aid, byte[] details, byte[] picture)
        {
            Aid = aid;
            Details = details;
            Picture = picture;
        }

        public void SaveToFile(string filePath)
        {
            try
            {
                using (Stream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    int sizeOfHeader = Marshal.SizeOf(typeof(PersistentFileHeader));
                    PersistentFileHeader header = new PersistentFileHeader();
                    header.Magic = PersistentFileHeader.MagicKey;
                    header.Version = PersistentFileHeader.CurrentVersion;
                    header.Aid = Aid;
                    header.DetailsLength = (ulong)Details.Length;
                    if (Picture != null && Picture.Length > 0)
                        header.PictureLength = (ulong)Picture.Length;
                    else
                        header.PictureLength = 0;
                    fileStream.WriteStruct(header);

                    fileStream.Write(Details, 0, Details.Length);

                    if (header.PictureLength > 0)
                        fileStream.Write(Picture, 0, Picture.Length);

                    fileStream.Flush();
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed saving persistent anime file '{filePath}'!", e);
            }
        }

        public static PersistentAnime LoadFromFile(string filePath)
        {
            PersistentAnime result = null;
            try
            {
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    PersistentFileHeader header = stream.ReadStruct<PersistentFileHeader>();
                    if (header.Magic != PersistentFileHeader.MagicKey)
                        throw new Exception($"File '{filePath}' is not a valid onigiri persistent file!");
                    if (header.Aid == 0)
                        throw new Exception($"Wrong aid in persistence file '{filePath}'!");
                    if (header.DetailsLength == 0)
                        throw new Exception($"Missing details block in persistence file '{filePath}'!");

                    byte[] detailsData = new byte[header.DetailsLength];
                    stream.Read(detailsData, 0, (int)header.DetailsLength);

                    byte[] pictureData = null;
                    if (header.PictureLength > 0)
                    {
                        pictureData = new byte[header.PictureLength];
                        stream.Read(pictureData, 0, (int)header.PictureLength);
                    }

                    result = new PersistentAnime(header.Aid, detailsData, pictureData);
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed loading persistent anime file '{filePath}'!", e);
            }
            return (result);
        }
    }
}
