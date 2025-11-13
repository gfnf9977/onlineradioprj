using System.Threading.Tasks;

namespace OnlineRadioStation.Domain
{
    public interface IAudioStream
    {
        Task<string> CreateStreamAsync(string inputAudioPath);
        int GetBitrate();
    }
}