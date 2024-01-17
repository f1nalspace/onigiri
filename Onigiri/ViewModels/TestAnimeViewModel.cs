using Finalspace.Onigiri.Media;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.Types;
using System;

namespace Finalspace.Onigiri.ViewModels
{
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

            string loremIpson = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Praesent egestas est at tellus efficitur, rutrum fringilla lectus ultrices. Maecenas eget pretium arcu. Aenean venenatis consequat odio, eu mollis lectus euismod id. Duis varius dolor vestibulum rhoncus consectetur. Praesent aliquet diam sit amet tincidunt iaculis. Maecenas ornare placerat ligula, ac ultrices lorem consequat non. Integer porta viverra elit et aliquam. Vestibulum id felis dapibus, ullamcorper mi eu, iaculis lacus.\nCurabitur dictum arcu in purus dignissim aliquam.Sed pulvinar enim in maximus consequat. Phasellus sapien massa, porta at ultrices id, bibendum non sem. Proin accumsan risus vitae purus vestibulum, in feugiat enim fermentum.Cras non ex lobortis, euismod velit id, cursus diam.Suspendisse in varius massa, in feugiat odio. Maecenas sed consectetur diam, at imperdiet ligula. Donec vestibulum lectus semper, maximus risus blandit, interdum nibh.Integer est dolor, gravida a molestie a, finibus id urna. Nulla imperdiet accumsan pulvinar. Vestibulum ipsum risus, aliquet quis auctor ut, pellentesque eget est. Nam tincidunt dictum faucibus. Sed eu egestas diam. Fusce magna elit, tempor vel consequat et, fringilla at velit. Nullam purus lacus, mollis at eleifend id, suscipit commodo enim.\nSed dictum enim quis arcu tempor, vitae luctus augue aliquam.Aliquam eleifend nisl ut enim porta, vitae congue libero hendrerit.Nullam libero ex, condimentum ac eros a, iaculis efficitur leo. Interdum et malesuada fames ac ante ipsum primis in faucibus.Quisque dolor nisi, molestie sed commodo at, convallis eu mi. Curabitur ut massa est. Cras at diam mattis, sollicitudin lorem quis, tincidunt massa.Phasellus blandit suscipit metus sit amet mattis.Integer luctus viverra rhoncus. Fusce congue tortor dapibus tortor efficitur, lobortis consequat odio rhoncus.Pellentesque facilisis quam ac fermentum iaculis. Aenean id neque odio. Morbi tempor velit purus, ac cursus justo eleifend a. Vestibulum ullamcorper lorem a purus tincidunt, eu ultrices tortor tempus.";

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

            mediaInfo.Video.Add(new VideoInfo()
            {
                Codec = new CodecDescription(FourCC.FromString("h274"), "H.264"),
                FrameCount = 1337,
                FrameRate = 25.0,
                Width = 1920,
                Height = 1080,
                Name = "Video1",
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

            AddonData.Deleteits.Add(new UserState() { UserName = "final", Value = false });
            AddonData.Deleteits.Add(new UserState() { UserName = "anni", Value = true });
            AddonData.Watchstates.Add(new UserState() { UserName = "final", Value = true });
            AddonData.Watchstates.Add(new UserState() { UserName = "anni", Value = false });
            AddonData.Marked = true;
        }
    }
}
