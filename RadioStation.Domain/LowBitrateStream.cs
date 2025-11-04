namespace OnlineRadioStation.Domain
{
    public class LowBitrateStream : IAudioStream
    {
        public int GetBitrate() => 64;
        public string StreamTrack(string title) => $"[64kb/s] Стрім: {title}";
    }
}