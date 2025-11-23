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

        private async Task<string> ConvertFileInternal(string tempFilePath)
        {
            int[] targetBitrates = { 64, 92, 128, 196, 224 };
            var masterPlaylistContent = "#EXTM3U\n#EXT-X-VERSION:3\n";
            var baseFileName = Path.GetFileNameWithoutExtension(tempFilePath);
            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "streams", baseFileName);
            Directory.CreateDirectory(baseFolder);

            foreach (var bitrate in targetBitrates)
            {
                var streamProduct = _streamFactory.Create(bitrate);
                await streamProduct.CreateStreamAsync(tempFilePath, bitrate.ToString());
                masterPlaylistContent += $"#EXT-X-STREAM-INF:BANDWIDTH={bitrate * 1000},CODECS=\"mp4a.40.2\"\n";
                masterPlaylistContent += $"{bitrate}/index.m3u8\n";
            }

            var masterPath = Path.Combine(baseFolder, "master.m3u8");
            await File.WriteAllTextAsync(masterPath, masterPlaylistContent);
            return $"/streams/{baseFileName}/master.m3u8";
        }

        public async Task ProcessNewTrackAsync(string tempFilePath, string title, Guid stationId, Guid djId)
        {
            var finalHlsUrl = await ConvertFileInternal(tempFilePath);
            var realDuration = await _converter.GetTrackDurationAsync(tempFilePath);

            var newTrack = new Track
            {
                TrackId = Guid.NewGuid(),
                Title = title,
                Duration = realDuration,
                UploadedById = djId,
                HlsUrl = finalHlsUrl
            };

            _trackRepository.AddEntity(newTrack);

            var nextPosition = _queueRepository.GetAll().Count(q => q.StationId == stationId) + 1;
            var newQueueEntry = new PlaybackQueue
            {
                QueueId = Guid.NewGuid(),
                TrackId = newTrack.TrackId,
                StationId = stationId,
                AddedById = djId,
                QueuePosition = nextPosition
            };

            _queueRepository.AddEntity(newQueueEntry);
            await _trackRepository.SaveChangesAsync();
        }

        public async Task UploadToLibraryAsync(string tempFilePath, string title, Guid adminId)
        {
            var finalHlsUrl = await ConvertFileInternal(tempFilePath);
            var realDuration = await _converter.GetTrackDurationAsync(tempFilePath);

            var newTrack = new Track
            {
                TrackId = Guid.NewGuid(),
                Title = title,
                Duration = realDuration,
                UploadedById = adminId,
                HlsUrl = finalHlsUrl
            };

            _trackRepository.AddEntity(newTrack);
            await _trackRepository.SaveChangesAsync();
        }
    }
}
