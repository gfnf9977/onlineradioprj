namespace OnlineRadioStation.Domain
{
    public interface IStationPlaylist
    {
        IPlaylistIterator CreateIterator();
    }
}
