using System;
using System.Collections.Immutable;

namespace Finalspace.Onigiri.Models
{
    public class AnimeImage
    {
        public string FileName { get; }
        public ImmutableArray<byte> Data { get; }

        public AnimeImage(string fileName, ImmutableArray<byte> data)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            FileName = fileName;
            Data = data;
        }

        public AnimeImage(string fileName, byte[] data)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            FileName = fileName;
            Data = data.ToImmutableArray();
        }
    }
}
