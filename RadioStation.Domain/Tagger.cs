using System.IO;

namespace OnlineRadioStation.Domain
{
    public class Tagger
    {
        public string AddTags(string inputPath)
        {
            Console.WriteLine($"[Tagger] Додаю метадані: {Path.GetFileName(inputPath)}");
            return inputPath + ".tagged.aac";
        }
    }
}