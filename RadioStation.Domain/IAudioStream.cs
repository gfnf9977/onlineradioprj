namespace OnlineRadioStation.Domain
{
    public interface IAudioStream
    {
        int GetBitrate();
        string StreamTrack(string title);
    }
}