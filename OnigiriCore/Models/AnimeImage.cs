using System;
using System.Collections.Immutable;

namespace Finalspace.Onigiri.Models
{
    public class AnimeImage
    {
        public string FileName { get; }
        public ImmutableArray<byte> Data { get; }

        public AnimeImage(string fileName, ReadOnlySpan<byte> data)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            FileName = fileName;
            Data = data.ToImmutableArray();
        }
    }
}
