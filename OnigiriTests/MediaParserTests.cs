using Finalspace.Onigiri.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnigiriTests
{
    [TestClass]
    public class MediaParserTests
    {
        [TestMethod]
        public void TestAvi()
        {
            string usersPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string folderPath = Path.Combine(usersPath, "OneDrive", "Q3");

            DirectoryInfo folder = new DirectoryInfo(folderPath);
            if (!folder.Exists)
                Assert.Fail($"Folder path '{folderPath}' does not exists");

            FileInfo[] files = folder
                .GetFiles("*.avi", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f.Name)
                .ToArray();

            foreach (FileInfo file in files)
            {
                Task<MediaInfo> task = MediaInfoParser.Parse(file);
                task.Wait();
                MediaInfo info = task.Result;
                Assert.IsNotNull(info);

                VideoInfo video = info.Video.FirstOrDefault();
                Assert.IsNotNull(video);
                Assert.IsTrue(video.Width > 0 && video.Height > 0 && !video.Codec.Id.IsEmpty);

                AudioInfo audio = info.Audio.FirstOrDefault();
                Assert.IsNotNull(audio);
                Assert.IsTrue(audio.Channels > 0 && audio.SampleRate > 0 && !audio.Codec.Id.IsEmpty);
            }

        }
    }
}
