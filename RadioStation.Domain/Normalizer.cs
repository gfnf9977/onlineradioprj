using System.IO;

namespace OnlineRadioStation.Domain
{
    public class Normalizer
    {
        public string Normalize(string inputPath)
        {
            Console.WriteLine($"[Normalizer] Нормалізую: {Path.GetFileName(inputPath)}");
            return inputPath + ".norm.mp3";
        }
    }
}