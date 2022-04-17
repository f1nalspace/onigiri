namespace Finalspace.Onigiri.Types
{
    public struct SubtitleInfo
    {
        public string Lang { get; }

        public SubtitleInfo(string language)
        {
            Lang = language;
        }
    }
}
