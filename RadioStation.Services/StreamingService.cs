// Services/StreamingService.cs

using OnlineRadioStation.Domain;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public class StreamingService
    {
        // ЛР5: Adapter
        private readonly IAudioConverter? _converter;

        // ЛР7: Facade
        private readonly IAudioProcessor? _audioProcessor;

        public StreamingService(
            IAudioConverter? converter = null,
            IAudioProcessor? audioProcessor = null)
        {
            _converter = converter;
            _audioProcessor = audioProcessor;
        }

        // ЛР4: Iterator
        public void StartStreaming(PlaybackQueue queue)
        {
            var iterator = queue.CreateIterator();
            iterator.First();

            Console.WriteLine("=== Початок стримінгу (Iterator) ===");
            while (!iterator.IsDone())
            {
                var track = iterator.Current();
                Console.WriteLine($"Відтворюється: {track.Title} ({track.Duration})");
                iterator.Next();
            }
            Console.WriteLine("=== Стрім завершено ===");
        }

        // ЛР5: Adapter
        public async Task<string> CreateHlsStreamAsync(string mp3Path, int bitrate = 128)
        {
            if (_converter == null)
                throw new InvalidOperationException("IAudioConverter не зареєстровано в DI");

            return await _converter.ConvertToHlsAsync(mp3Path, bitrate);
        }

        // ЛР6: Factory Method
        public string StartStreamWithFactory(int bitrate, string title)
        {
            var factory = new BitrateStreamFactory();
            var stream = factory.Create(bitrate);
            return stream.StreamTrack(title);
        }

        // ЛР7: Facade
        public string PrepareTrack(string mp3Path)
        {
            if (_audioProcessor == null)
                throw new InvalidOperationException("IAudioProcessor не зареєстровано в DI");

            return _audioProcessor.Process(mp3Path);
        }
    }
}