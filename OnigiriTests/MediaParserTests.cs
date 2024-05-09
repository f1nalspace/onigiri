using Finalspace.Onigiri;
using Finalspace.Onigiri.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnigiriTests
{
    [TestClass]
    public class MediaParserTests
    {
        static IEnumerable<FileInfo> AddFilesFromFolder(DirectoryInfo rootDir)
        {
            if (!rootDir.Attributes.HasFlag(FileAttributes.System))
            {
                FileInfo[] subFiles = rootDir.GetFiles();
                foreach (FileInfo subFile in subFiles)
                {
                    if (subFile.Attributes.HasFlag(FileAttributes.System))
                        continue;
                    string ext = subFile.Extension.ToLower();
                    if (OnigiriService.MediaFileExtensions.Contains(ext))
                        yield return subFile;
                }

                DirectoryInfo[] subdirs = rootDir.GetDirectories();
                foreach (DirectoryInfo subdir in subdirs)
                {
                    IEnumerable<FileInfo> subfiles = AddFilesFromFolder(subdir);
                    foreach (FileInfo subfile in subfiles)
                        yield return subfile;
                }
            }
        }

        [TestMethod]
        public void TestAvi()
        {
            string usersPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string folderPath = Path.Combine(usersPath, "OneDrive", "Q3");

            DirectoryInfo folder = new DirectoryInfo(folderPath);
            if (!folder.Exists)
                Assert.Fail($"Folder path '{folderPath}' does not exists");

            List<FileInfo> files = AddFilesFromFolder(folder).ToList();

            foreach (FileInfo file in files)
            {
                Task<MediaInfo> task = MediaInfoParser.Parse(file);
                task.Wait();
                MediaInfo info = task.Result;
                Assert.IsNotNull(info);

                Console.WriteLine(FormattableString.Invariant($"\tContainer: '{info.Format}', Duration: {info.Duration.TotalSeconds} secs"));

                foreach (VideoInfo video in info.Video)
                {
                    Assert.IsNotNull(video);
                    Console.WriteLine(FormattableString.Invariant($"\tVideo: {video.Width}x{video.Height}, {video.FrameCount} frames, {video.FrameRate} fps [Codec:'{video.Codec}', Name: '{video.Name}']"));
                    Assert.IsTrue(video.Width > 0 && video.Height > 0 && !video.Codec.Id.IsEmpty);
                }

                foreach (AudioInfo audio in info.Audio)
                {
                    Assert.IsNotNull(audio);
                    Console.WriteLine(FormattableString.Invariant($"\tAudio: {audio.Channels} channels, {audio.SampleRate} Hz, {audio.BitsPerSample} bits/sample, {audio.BitRate / 1000} kHz [Codec: '{audio.Codec}', Name: '{audio.Name}']"));
                    Assert.IsTrue(audio.Channels > 0 && audio.SampleRate > 0 && !audio.Codec.Id.IsEmpty);
                }
            }
        }
    }
}
