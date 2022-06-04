namespace Finalspace.Onigiri.Types
{
    public static class QualityLevel
    {
        public const int Quality_SD_576p = 576;
        public const int Quality_HD_720p = 720;
        public const int Quality_FullHD_1080 = 1080;
        public const int Quality_QHD_1440p = 1440;
        public const int Quality_4K_2160p = 1440;

        public static readonly int[] AllLevels = new int[] {
            Quality_SD_576p,
            Quality_HD_720p,
            Quality_FullHD_1080,
            Quality_QHD_1440p,
            Quality_4K_2160p
        };

        public static int HeightToQualityLevel(int height)
        {
            int result = Quality_SD_576p;
            for (int i = AllLevels.Length - 1; i >= 0; i--)
            {
                if (height >= AllLevels[i])
                    return AllLevels[i];
            }
            return result;
        }
    }
}
