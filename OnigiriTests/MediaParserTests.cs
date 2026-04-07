using Finalspace.Onigiri;
using Finalspace.Onigiri.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

using Xunit.Abstractions;

namespace OnigiriTests;

public class MediaParserTests(ITestOutputHelper output)
{
    private static IEnumerable<FileInfo> AddFilesFromFolder(DirectoryInfo rootDir)
    {
        if (rootDir.Attributes.HasFlag(FileAttributes.System))
            yield break;

        var files = rootDir.EnumerateFiles("*", SearchOption.AllDirectories)
            .Where(f => !f.Attributes.HasFlag(FileAttributes.System))
            .Where(f => OnigiriService.MediaFileExtensions.Contains(f.Extension.ToLowerInvariant()));

        foreach (var file in files)
        {
            yield return file;
        }
    }

    [Fact]
    public async Task TestAvi()
    {
        string usersPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string folderPath = Path.Combine(usersPath, "OneDrive", "Q3");

        DirectoryInfo folder = new DirectoryInfo(folderPath);
        if (!folder.Exists)
        {
            output.WriteLine($"Folder path '{folderPath}' does not exists, skipping test.");
            return;
        }

        var files = AddFilesFromFolder(folder).ToList();

        foreach (FileInfo file in files)
        {
            MediaInfo info = await MediaInfoParser.Parse(file);
            Assert.NotNull(info);

            output.WriteLine(FormattableString.Invariant($"\tContainer: '{info.Format}', Duration: {info.Duration.TotalSeconds} secs"));

            foreach (VideoInfo video in info.Video)
            {
                Assert.NotNull(video);
                output.WriteLine(FormattableString.Invariant($"\tVideo: {video.Width}x{video.Height}, {video.FrameCount} frames, {video.FrameRate} fps [Codec:'{video.Codec}', Name: '{video.Name}']"));
                Assert.True(video.Width > 0 && video.Height > 0 && !video.Codec.Id.IsEmpty);
            }

            foreach (AudioInfo audio in info.Audio)
            {
                Assert.NotNull(audio);
                output.WriteLine(FormattableString.Invariant($"\tAudio: {audio.Channels} channels, {audio.SampleRate} Hz, {audio.BitsPerSample} bits/sample, {audio.BitRate / 1000} kHz [Codec: '{audio.Codec}', Name: '{audio.Name}']"));
                Assert.True(audio.Channels > 0 && audio.SampleRate > 0 && !audio.Codec.Id.IsEmpty);
            }
        }
    }
}