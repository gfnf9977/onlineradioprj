namespace OnlineRadioStation.Domain
{
    public class BitrateStreamFactory : StreamFactory
    {
        public override IAudioStream Create(int bitrate)
        {
            return bitrate switch
            {
                <= 64 => new LowBitrateStream(),
                <= 128 => new StandardBitrateStream(),
                _ => new HighBitrateStream()
            };
        }
    }
}