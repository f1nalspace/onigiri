using System;
using System.Collections.Immutable;

namespace Finalspace.Onigiri.Models
{
    public class AnimeImage
    {
        public int Width { get; }
        public int Height { get; }
        public string FileName { get; }
        public ImmutableArray<byte> Data { get; }

        public AnimeImage(int width, int height, string fileName, ImmutableArray<byte> data)
        {
            if (width <= 0)
                throw new ArgumentException($"Width of '{width}' is invalid", nameof(width));
            if (height <= 0)
                throw new ArgumentException($"Height of '{height}' is invalid", nameof(height));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            Width = width;
            Height = height;
            Data = data;
        }
    }
}
