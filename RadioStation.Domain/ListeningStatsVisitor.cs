namespace OnlineRadioStation.Domain
{
    public class ListeningStatsVisitor : IStatsVisitor
    {
        public int TotalListenedMinutes { get; private set; } = 0;
        public int TotalSessions { get; private set; } = 0;
        public int TotalTracks { get; private set; } = 0;

        public void VisitTrack(Track track)
        {
            TotalListenedMinutes += (int)track.Duration.TotalMinutes;
            Console.WriteLine($"[Stats] Тривалість треку {track.Title}: {track.Duration.TotalMinutes} хв");
        }

        public void VisitStream(DjStream stream)
        {
            TotalSessions += 1;
            Console.WriteLine($"[Stats] Сесія стріму {stream.StreamId}: {stream.EndTime - stream.StartTime}");
        }

        public void VisitQueue(PlaybackQueue queue)
        {
            TotalTracks += 1; // кількість треків у черзі
            Console.WriteLine($"[Stats] Черга {queue.QueueId}: {queue.QueuePosition} позиція");
        }
    }
}