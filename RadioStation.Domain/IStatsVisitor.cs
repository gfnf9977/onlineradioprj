namespace OnlineRadioStation.Domain
{
    public interface IStatsVisitor
    {
        void VisitTrack(Track track);
        void VisitStream(DjStream stream);
        void VisitQueue(PlaybackQueue queue);
    }
}