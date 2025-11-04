namespace OnlineRadioStation.Domain
{
    public class StandardBitrateStream : IAudioStream
    {
        public int GetBitrate() => 128;
        public string StreamTrack(string title) => $"[128kb/s] Стрім: {title}";
    }
}