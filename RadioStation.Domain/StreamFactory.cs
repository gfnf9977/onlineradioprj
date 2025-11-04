namespace OnlineRadioStation.Domain
{
    public abstract class StreamFactory
    {
        public abstract IAudioStream Create(int bitrate);
    }
}