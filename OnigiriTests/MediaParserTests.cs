using Finalspace.Onigiri.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
            string usersPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string folderPath = Path.Combine(usersPath, "OneDrive", "Q3");

            DirectoryInfo folder = new DirectoryInfo(folderPath);
            if (!folder.Exists)
                Assert.Fail($"Folder path '{folderPath}' does not exists");

            FileInfo[] files = folder
                .GetFiles("*.avi", SearchOption.TopDirectoryOnly)
                .OrderBy(f => f.Name)
                .ToArray();

            foreach (var file in files)
            {
                var task = MediaInfoParser.Parse(file);
                task.Wait();
                MediaInfo info = task.Result;
                Assert.IsNotNull(info);
            }

        }
    }
}
