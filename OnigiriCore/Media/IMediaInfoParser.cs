using System.Threading.Tasks;

namespace Finalspace.Onigiri.Media
{
    public interface IMediaInfoParser
    {
        Task<MediaInfo> Parse(string filePath);
    }
}
