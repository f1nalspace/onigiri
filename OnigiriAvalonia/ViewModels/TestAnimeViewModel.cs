using Finalspace.Onigiri.Media;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.Types;
using System;

namespace Finalspace.Onigiri.ViewModels;

public class TestAnimeViewModel : Anime
{
    public TestAnimeViewModel() : base()
    {
        Titles.Add(new Title() { Name = "Lorem ipsum dolor sit amet", Type = "main" });

        Ratings.Add(new Rating() { Count = 1000, Value = 7.3, Name = RatingTypes.Pernament });

        TopCategories.Add(new Category() { Name = "Blubb", Weight = 9 });
        TopCategories.Add(new Category() { Name = "Mystery", Weight = 8 });
        TopCategories.Add(new Category() { Name = "Violence", Weight = 7 });
        TopCategories.Add(new Category() { Name = "Action", Weight = 6 });
        TopCategories.Add(new Category() { Name = "Original", Weight = 5 });
        TopCategories.Add(new Category() { Name = "Girls", Weight = 4 });
        TopCategories.Add(new Category() { Name = "Isekai", Weight = 3 });
        TopCategories.Add(new Category() { Name = "Again", Weight = 2 });
        TopCategories.Add(new Category() { Name = "Strange", Weight = 1 });
        TopCategories.Add(new Category() { Name = "Weird", Weight = 0 });
        TopCategories.Add(new Category() { Name = "Rofl", Weight = 4 });
        TopCategories.Add(new Category() { Name = "Whatever", Weight = 7 });
        TopCategories.Add(new Category() { Name = "NotGreat", Weight = 0 });
        TopCategories.Add(new Category() { Name = "Hopefully", Weight = 2 });

        string loremIpson = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent egestas est at tellus efficitur, rutrum fringilla lectus ultrices.";

        Type = "OVA";

        EpCount = 16;

        StartDate = DateTime.Now;
        EndDate = DateTime.Now.AddMonths(6);

        Description = loremIpson.Replace("\n", Environment.NewLine + Environment.NewLine);

        var mediaInfo = new MediaInfo()
        {
            Format = new CodecDescription(FourCC.FromString("avi"), "AVI-Container"),
            Duration = TimeSpan.FromSeconds(1337.0),
        };

        mediaInfo.Video.Add(new VideoInfo()
        {
            Codec = new CodecDescription(FourCC.FromString("h274"), "H.264"),
            FrameCount = 1337,
            FrameRate = 25.0,
            Width = 1280,
            Height = 720,
            Name = "Video0",
        });

        mediaInfo.Audio.Add(new AudioInfo()
        {
            Codec = new CodecDescription(FourCC.FromString("h274"), "H.264"),
            FrameCount = 1337,
            BitRate = 138000,
            BitsPerSample = 16,
            Channels = 2,
            SampleRate = 44110,
            Lang = LanguageNames.JapaneseShort,
            Name = "Audio",
        });

        mediaInfo.Subtitles.Add(new SubtitleInfo()
        {
            Lang = LanguageNames.EnglishShort,
            Name = "Subtitle",
        });

        ExtendedMediaFiles.Add(new AnimeMediaFile()
        {
            FileName = "anime.avi",
            FileSize = 1337128,
            Info = mediaInfo,
        });

        AddonData.Deleteits.Add(new UserState() { UserName = "final", Value = true });
        AddonData.Deleteits.Add(new UserState() { UserName = "anni", Value = true });
        AddonData.Watchstates.Add(new UserState() { UserName = "final", Value = true });
        AddonData.Watchstates.Add(new UserState() { UserName = "anni", Value = true });
        AddonData.Marked = true;
    }
}
