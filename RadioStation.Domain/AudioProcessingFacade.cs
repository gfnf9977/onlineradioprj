using System.IO;

namespace OnlineRadioStation.Domain
{
    public class AudioProcessingFacade : IAudioProcessor
    {
        private readonly Normalizer _normalizer;
        private readonly Encoder _encoder;
        private readonly Tagger _tagger;

        public AudioProcessingFacade(Normalizer normalizer, Encoder encoder, Tagger tagger)
        {
            _normalizer = normalizer;
            _encoder = encoder;
            _tagger = tagger;
        }

        public string Process(string inputPath)
        {
            var step1 = _normalizer.Normalize(inputPath);
            var step2 = _encoder.Encode(step1);
            var result = _tagger.AddTags(step2);

            Console.WriteLine($"[Facade] Обробка завершена: {Path.GetFileName(result)}");
            return result;
        }
    }
}