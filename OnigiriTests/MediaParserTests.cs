using Finalspace.Onigiri.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OnigiriTests
{
    [TestClass]
    public class MediaParserTests
    {
        [TestMethod]
        public void TestAvi()
        {
            MediaInfo ep1 = MediaInfoParser.Parse(@"X:\Argento Soma\[E-F] Argento Soma - ep01 [05429A44].avi");
            Assert.IsNotNull(ep1);
        }
    }
}
