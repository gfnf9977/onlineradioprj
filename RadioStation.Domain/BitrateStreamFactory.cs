namespace OnlineRadioStation.Domain
{
    public class BitrateStreamFactory : StreamFactory
    {
        private readonly IAudioConverter _converter;

        public BitrateStreamFactory(IAudioConverter converter)
        {
            _converter = converter;
        }

        public override IAudioStream Create(int bitrate)
        {
            return bitrate switch
            {
                <= 64 => new LowBitrateStream(_converter),
                <= 128 => new StandardBitrateStream(_converter),
                _ => new HighBitrateStream(_converter)
            };
        }
    }
}