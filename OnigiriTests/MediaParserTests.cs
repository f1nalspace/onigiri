using Finalspace.Onigiri.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace OnigiriTests
{
    [TestClass]
    public class MediaParserTests
    {
        [TestMethod]
        public void TestAvi()
        {
            string folderPath = @"E:\Gamevideos";

            DirectoryInfo folder = new DirectoryInfo(folderPath);
            if (!folder.Exists)
                Assert.Fail($"Folder path '{folderPath}' does not exists");

            FileInfo[] files = folder
                .GetFiles("*.avi", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f.Name)
                .ToArray();

            foreach (var file in files)
            {
                MediaInfo info = MediaInfoParser.Parse(file);
                Assert.IsNotNull(info);
            }

        }
    }
}
