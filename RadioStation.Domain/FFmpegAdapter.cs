using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OnlineRadioStation.Domain
{
    public class FFmpegAdapter : IAudioConverter
    {
        private const string FfmpegPath = "wwwroot/ffmpeg/ffmpeg.exe";

        public async Task<string> ConvertToHlsAsync(string inputPath, int bitrate)
        {
            var outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "streams", Path.GetFileNameWithoutExtension(inputPath));
            Directory.CreateDirectory(outputFolder);
            var playlist = Path.Combine(outputFolder, "stream.m3u8");

            var args = $"-i \"{inputPath}\" " +
                       $"-c:a aac -b:a {bitrate}k " +
                       $"-f hls -hls_time 10 -hls_list_size 0 " +
                       $"-hls_segment_filename \"{outputFolder}/seg%03d.ts\" " +
                       $"\"{playlist}\"";

            var psi = new ProcessStartInfo
            {
                FileName = FfmpegPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)!;
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new System.Exception(await process.StandardError.ReadToEndAsync());

            return playlist;
        }
    }
}