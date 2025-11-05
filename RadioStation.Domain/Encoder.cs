using System.IO;

namespace OnlineRadioStation.Domain
{
    public class Encoder
    {
        public string Encode(string inputPath)
        {
            Console.WriteLine($"[Encoder] Кодую в AAC: {Path.GetFileName(inputPath)}");
            return inputPath + ".aac";
        }
    }
}