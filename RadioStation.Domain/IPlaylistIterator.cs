namespace OnlineRadioStation.Domain
{
    public interface IPlaylistIterator
    {
        void First();
        void Next();
        bool IsDone();
        Track Current();
    }
}