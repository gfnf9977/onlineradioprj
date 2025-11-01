using OnlineRadioStation.Domain;

namespace OnlineRadioStation.Services
{
    public class StreamingService
    {
        public void StartStreaming(PlaybackQueue queue)
        {
            var iterator = queue.CreateIterator();
            iterator.First();

            Console.WriteLine("=== Початок стримінгу ===");
            while (!iterator.IsDone())
            {
                var track = iterator.Current();
                Console.WriteLine($"Відтворюється: {track.Title} ({track.Duration})");
                // Тут буде FFmpeg
                iterator.Next();
            }
            Console.WriteLine("=== Стрім завершено ===");
        }
    }
}