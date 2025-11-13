using System.Diagnostics;
using System.IO;
using System.Text;
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

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            using var process = new Process { StartInfo = psi };

            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null) outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null) errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var errorMessage = errorBuilder.ToString();
                if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "FFmpeg failed with exit code " + process.ExitCode + ". Output: " + outputBuilder.ToString();
                }
                Console.WriteLine("FFMPEG ERROR: " + errorMessage);
                throw new System.Exception("FFmpeg error: " + errorMessage);
            }

            Console.WriteLine("FFMPEG LOG: " + errorBuilder.ToString());

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var webPath = "/" + Path.GetRelativePath(webRootPath, playlist).Replace("\\", "/");
            return webPath;
        }
    }
}
