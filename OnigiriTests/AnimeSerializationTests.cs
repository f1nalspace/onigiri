using Finalspace.Onigiri.Media;
using Finalspace.Onigiri.Models;
using Finalspace.Onigiri.Storage;
using Finalspace.Onigiri.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnigiriTests
{
    [TestClass]
    public class AnimeSerializationTests
    {
        [TestMethod]
        public void SerializeTest()
        {
            Anime source = new Anime()
            {
                Aid = 2337,
                Description = "My description",
                EpCount = 1,
                StartDate = new DateTime(2002, 05, 07, 11, 33, 21),
                EndDate = new DateTime(2003, 09, 05, 13, 05, 44),
                Type = "OVA",
            };

            source.Categories.Add(new Category()
            {
                Id = 1,
                Description = "Extreme violence",
                Name = "Violence",
                ParentId = 0,
                Weight = 10,
            });
            source.Categories.Add(new Category()
            {
                Id = 2,
                Description = "Comedy",
                Name = "Comedy",
                ParentId = 0,
                Weight = 15,
            });
            source.Categories.Add(new Category()
            {
                Id = 2,
                Description = "Bad people",
                Name = "Bad",
                ParentId = 0,
                Weight = 40,
            });

            source.Ratings.Add(new Rating()
            {
                Count = 1024,
                Name = RatingTypes.Pernament,
                Value = 7.32,
            });

            source.Episodes.Add(new Episode()
            {
                Id = 1,
                AirDate = DateTime.Now,
                Length = 333,
                Num = "First",
                NumType = 0,
                Rating = new Rating() { Count = 233, Name = RatingTypes.Pernament, Value = 4.3 },
                Titles = new ObservableCollection<Title>(),
            });

            source.Titles.Add(new Title()
            {
                Aid = source.Aid,
                Lang = LanguageNames.EnglishShort,
                Type = TitleTypes.Main,
                Name = "The anime name",
            });

            IEnumerable<Category> topCats = source.Categories.OrderByDescending(d => d.Weight).Take(Math.Min(10, source.Categories.Count));
            foreach (Category item in topCats)
                source.TopCategories.Add(item);

            var mediaFile = new AnimeMediaFile()
            {
                FileName = "the_anime.avi",
                FileSize = 1337,
                Info = new MediaInfo()
                {
                    Audio = new List<AudioInfo>(new[]
                    {
                        new AudioInfo()
                        {
                            Channels = 2,
                            SampleRate = 44100,
                            FrameCount = 2337,
                            BitRate = 128,
                            BitsPerSample = 16,
                            Codec = new CodecDescription(FourCC.FromString("mp3"), "Mpeg layer-3"),
                            Lang = LanguageNames.JapaneseShort,
                            Name = "AudioTrack",
                        }
                    }),
                    Video = new List<VideoInfo>(new[]
                    {
                        new VideoInfo()
                        {
                            FrameCount = 289,
                            Codec = new CodecDescription(FourCC.FromString("xvid"), "XViD"),
                            Name = "VideoTrack",
                            FrameRate = 23.335,
                            Height = 720,
                            Width = 1280,
                        }
                    }),
                    Subtitles = new List<SubtitleInfo>(new[]
                    {
                        new SubtitleInfo()
                        {
                            Name = "Subtitle",
                            Lang = LanguageNames.EnglishShort,
                        }
                    })
                },
            };

            source.ExtendedMediaFiles.Add(mediaFile);
            source.MediaFiles = source.ExtendedMediaFiles.Select(s => s.FileName).ToList();

            Anime target;
            using (MemoryStream stream = AnimeSerialization.SerializeAnime(source))
                target = AnimeSerialization.DeserializeAnime(stream.GetBuffer(), source.Aid);

            Assert.IsNotNull(target);
            Assert.AreEqual(source.Aid, target.Aid);
            Assert.AreEqual(source.MainTitle, target.MainTitle);

            Assert.AreEqual(source.MediaFiles.Count, target.MediaFiles.Count);
            for (int i = 0; i < source.MediaFiles.Count; i++)
                Assert.AreEqual(source.MediaFiles[i], target.MediaFiles[i]);

            Assert.AreEqual(source.ExtendedMediaFiles.Count, target.ExtendedMediaFiles.Count);
            for (int i = 0; i < source.ExtendedMediaFiles.Count; i++)
            {
                AnimeMediaFile sourceMediaFile = source.ExtendedMediaFiles[i];

                AnimeMediaFile targetExtendedMediaFile = target.ExtendedMediaFiles[i];
                Assert.IsNotNull(targetExtendedMediaFile);

                Assert.AreEqual(sourceMediaFile.FileName, targetExtendedMediaFile.FileName);
                Assert.AreEqual(sourceMediaFile.FileSize, targetExtendedMediaFile.FileSize);

                MediaInfo sourceInfo = sourceMediaFile.Info;
                MediaInfo targetInfo = targetExtendedMediaFile.Info;
                Assert.IsNotNull(targetInfo);

                Assert.AreEqual(sourceInfo.Duration, targetInfo.Duration);
                Assert.AreEqual(sourceInfo.Format, targetInfo.Format);

                Assert.AreEqual(sourceInfo.Video.Count, targetInfo.Video.Count);
                for (int videoIndex = 0; videoIndex < sourceInfo.Video.Count; videoIndex++)
                {
                    VideoInfo sourceVideo = sourceInfo.Video[videoIndex];
                    VideoInfo targetVideo = targetInfo.Video[videoIndex];
                    Assert.AreEqual(sourceVideo, targetVideo);
                }

                Assert.AreEqual(sourceInfo.Audio.Count, targetInfo.Audio.Count);
                for (int audioIndex = 0; audioIndex < sourceInfo.Audio.Count; audioIndex++)
                {
                    AudioInfo sourceAudio = sourceInfo.Audio[audioIndex];
                    AudioInfo targetAudio = targetInfo.Audio[audioIndex];
                    Assert.AreEqual(sourceAudio, targetAudio);
                }

                Assert.AreEqual(sourceInfo.Subtitles.Count, targetInfo.Subtitles.Count);
                for (int subtitleIndex = 0; subtitleIndex < sourceInfo.Subtitles.Count; subtitleIndex++)
                {
                    SubtitleInfo sourceSubtitle = sourceInfo.Subtitles[subtitleIndex];
                    SubtitleInfo targetSubtitle = targetInfo.Subtitles[subtitleIndex];
                    Assert.AreEqual(sourceSubtitle, targetSubtitle);
                }
            }
        }
    }
}
