using System.Threading.Tasks;

namespace OnlineRadioStation.Domain
{
    public class StandardBitrateStream : IAudioStream
    {
        private readonly IAudioConverter _converter;
        private const int Bitrate = 128;

        public StandardBitrateStream(IAudioConverter converter)
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