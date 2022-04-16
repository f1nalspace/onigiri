using Finalspace.Onigiri.Extensions;
using Finalspace.Onigiri.Models;
using log4net;
using System;
using System.Collections.Immutable;
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
    public class AnimeFile
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ulong Aid { get; }

        public ImmutableArray<byte> Details { get; }

        public ImmutableArray<byte> Picture { get; }

        public AnimeFile(ulong aid, ImmutableArray<byte> details, ImmutableArray<byte> picture)
        {
            Aid = aid;
            Details = details;
            Picture = picture;
        }

        public ExecutionResult SaveToStream(Stream stream, string streamName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(streamName))
                throw new ArgumentNullException(nameof(streamName));
            try
            {
                int sizeOfHeader = Marshal.SizeOf(typeof(AnimeFileHeader));
                AnimeFileHeader header = new AnimeFileHeader();
                header.Magic = AnimeFileHeader.MagicKey;
                header.Version = AnimeFileHeader.CurrentVersion;
                header.Aid = Aid;
                header.DetailsLength = (ulong)Details.Length;
                header.PictureLength = (ulong)Picture.Length;
                stream.WriteStruct(header);

                stream.Write(Details.AsSpan());

                if (Picture.Length > 0)
                    stream.Write(Picture.AsSpan());

                stream.Flush();
            }
            catch (Exception e)
            {
                Exception failed = new Exception($"Failed to save anime database '{Aid}' to stream '{streamName}'!", e);
                log.Error(failed.Message, failed);
                return new ExecutionResult(failed);
            }
            return new ExecutionResult();
        }

        public ExecutionResult SaveToFile(string filePath)
        {
            try
            {
                using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    ExecutionResult success = SaveToStream(stream, filePath);
                    if (!success.Success)
                        throw success.Error;
                }
            }
            catch (Exception e)
            {
                Exception failed = new Exception($"Failed to save anime database '{Aid}' to file '{filePath}'!", e);
                log.Error(failed.Message, failed);
                return new ExecutionResult(failed);
            }
            return new ExecutionResult();
        }

        public static Tuple<ExecutionResult, AnimeFile> LoadFromStream(Stream stream, string streamName)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(streamName))
                throw new ArgumentNullException(nameof(streamName));
            AnimeFile animeFile = null;
            try
            {
                AnimeFileHeader header = stream.ReadStruct<AnimeFileHeader>();
                if (header.Magic != AnimeFileHeader.MagicKey)
                    throw new FormatException($"Wrong magic key");
                if (header.Aid == 0)
                    throw new FormatException($"Wrong aid in anime file stream");
                if (header.DetailsLength == 0)
                    throw new FormatException($"Missing details block");

                byte[] detailsData = new byte[header.DetailsLength];
                stream.Read(detailsData, 0, (int)header.DetailsLength);

                byte[] pictureData = null;
                if (header.PictureLength > 0)
                {
                    pictureData = new byte[header.PictureLength];
                    stream.Read(pictureData, 0, (int)header.PictureLength);
                }

                animeFile = new AnimeFile(header.Aid, detailsData.ToImmutableArray(), pictureData?.ToImmutableArray() ?? ImmutableArray<byte>.Empty);
            }
            catch (Exception e)
            {
                Exception failed = new Exception($"Failed to load anime database from stream '{stream}'!", e);
                log.Error(failed.Message, failed);
                return new Tuple<ExecutionResult, AnimeFile>(new ExecutionResult(failed), null);

            }
            return new Tuple<ExecutionResult, AnimeFile>(new ExecutionResult(), animeFile);
        }

        public static Tuple<ExecutionResult, AnimeFile> LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            AnimeFile animeFile = null;
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File '{filePath}' was not found", filePath);
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    Tuple<ExecutionResult, AnimeFile> res = LoadFromStream(stream, filePath);
                    if (!res.Item1.Success)
                        throw res.Item1.Error;
                    animeFile = res.Item2;
                }
            }
            catch (Exception e)
            {
                Exception failed = new Exception($"Failed to load anime database file '{filePath}'!", e);
                log.Error(failed.Message, e);
                return new Tuple<ExecutionResult, AnimeFile>(new ExecutionResult(failed), null);
            }
            return new Tuple<ExecutionResult, AnimeFile>(new ExecutionResult(), animeFile);
        }
    }
}
