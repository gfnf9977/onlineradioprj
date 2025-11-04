namespace OnlineRadioStation.Domain
{
    public class HighBitrateStream : IAudioStream
    {
        public int GetBitrate() => 224;
        public string StreamTrack(string title) => $"[224kb/s] Стрім: {title}";
    }
}