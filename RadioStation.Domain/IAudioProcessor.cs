using System.Threading.Tasks;

namespace OnlineRadioStation.Domain
{
    public interface IAudioConverter
    {
        Task<string> ConvertToHlsAsync(string inputPath, int bitrate);
    }
}