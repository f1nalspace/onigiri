using System.IO;
using System.Text;

namespace Finalspace.Onigiri.Utils
{
    public class TextContent
    {
        public Encoding Encoding { get; set; }
        public string Text { get; set; }

        public void SaveToFile(string filePath)
        {
            using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(stream, Encoding))
                {
                    writer.Write(Text);
                }
            }
        }
    }
}
