using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRadioStation.Domain
{
    public class AudioProcessingFacade : IAudioProcessor
    {
        private readonly IAudioConverter _converter;
        private readonly ITrackRepository _trackRepository;
        private readonly IPlaybackQueueRepository _queueRepository;
        private readonly StreamFactory _streamFactory;

        public AudioProcessingFacade(
            IAudioConverter converter,
            ITrackRepository trackRepository,
            IPlaybackQueueRepository queueRepository,
            StreamFactory streamFactory)
        {
            _converter = converter;
            _trackRepository = trackRepository;
            _queueRepository = queueRepository;
            _streamFactory = streamFactory;
        }

        public async Task ProcessNewTrackAsync(string tempFilePath, string title, Guid stationId, Guid djId, int bitrate)
        {
            Console.WriteLine($"[Facade] Починаю обробку: {title} з бітрейтом {bitrate}kb/s");

            var streamProduct = _streamFactory.Create(bitrate);
            Console.WriteLine($"[Facade] Викликаю {streamProduct.GetType().Name} (який викличе Adapter)...");

            var hlsWebUrl = await streamProduct.CreateStreamAsync(tempFilePath);
            Console.WriteLine($"[Facade] Adapter завершив роботу. HLS створено: {hlsWebUrl}");

            var hardcodedDuration = TimeSpan.FromMinutes(3);
            var newTrack = new Track
            {
                TrackId = Guid.NewGuid(),
                Title = title,
                Duration = hardcodedDuration,
                UploadedById = djId,
                HlsUrl = hlsWebUrl
            };
            _trackRepository.AddEntity(newTrack);
            Console.WriteLine($"[Facade] Додаю {newTrack.Title} в таблицю Tracks.");

            var nextPosition = _queueRepository.GetAll()
                .Count(q => q.StationId == stationId) + 1;
            var newQueueEntry = new PlaybackQueue
            {
                QueueId = Guid.NewGuid(),
                TrackId = newTrack.TrackId,
                StationId = stationId,
                AddedById = djId,
                QueuePosition = nextPosition
            };
            _queueRepository.AddEntity(newQueueEntry);
            Console.WriteLine($"[Facade] Додаю трек на позицію {nextPosition} в PlaybackQueue.");

            await _trackRepository.SaveChangesAsync();
            Console.WriteLine($"[Facade] Обробку треку '{title}' завершено.");
        }
    }
}
