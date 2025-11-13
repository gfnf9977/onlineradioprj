using System.Threading.Tasks;

namespace OnlineRadioStation.Domain
{
    public class LowBitrateStream : IAudioStream
    {
        private readonly IAudioConverter _converter;
        private const int Bitrate = 64;

        public LowBitrateStream(IAudioConverter converter)
        {
            _converter = converter;
        }

        public int GetBitrate() => Bitrate;

        public async Task<string> CreateStreamAsync(string inputAudioPath)
        {
            return await _converter.ConvertToHlsAsync(inputAudioPath, Bitrate);
        }
    }
}