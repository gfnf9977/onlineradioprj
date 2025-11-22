using System.Threading.Tasks;

namespace OnlineRadioStation.Domain
{
    public class HighBitrateStream : IAudioStream
    {
        private readonly IAudioConverter _converter;
        private const int Bitrate = 224;

        public HighBitrateStream(IAudioConverter converter)
        {
            _converter = converter;
        }

        public int GetBitrate() => Bitrate;

        public async Task<string> CreateStreamAsync(string inputAudioPath, string subfolder)
        {
            return await _converter.ConvertToHlsAsync(inputAudioPath, Bitrate, subfolder);
        }
    }
}