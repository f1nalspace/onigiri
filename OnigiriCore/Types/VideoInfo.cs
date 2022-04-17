namespace Finalspace.Onigiri.Types
{
    public struct VideoInfo
    {
        public int Width { get; }
        public int Height { get; }
        public double FrameRate { get; }
        public FourCC Codec { get; }

        public VideoInfo(int width, int height, double frameRate, FourCC codec)
        {
            Width = width;
            Height = height;
            FrameRate = frameRate;
            Codec = codec;
        }
    }
}
