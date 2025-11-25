using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRadioStation.Domain
{
    public class FFmpegAdapter : IAudioConverter
    {
        private const string FffmpegPath = "wwwroot/ffmpeg/ffmpeg.exe";
        private const string FfprobePath = "wwwroot/ffmpeg/ffprobe.exe";

        public async Task<string> ConvertToHlsAsync(string inputPath, int bitrate, string subfolder)
        {
            var baseFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "streams", Path.GetFileNameWithoutExtension(inputPath));
            var outputFolder = Path.Combine(baseFolder, subfolder);
            Directory.CreateDirectory(outputFolder);

            var playlistPath = Path.Combine(outputFolder, "index.m3u8");
            var segmentPath = Path.Combine(outputFolder, "seg%03d.ts");

            var args = $"-i \"{inputPath}\" " +
                       $"-c:a aac -b:a {bitrate}k " +
                       $"-f hls -hls_time 10 -hls_list_size 0 " +
                       $"-hls_segment_filename \"{segmentPath}\" " +
                       $"\"{playlistPath}\"";

            var psi = new ProcessStartInfo
            {
                FileName = FffmpegPath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            using var process = new Process { StartInfo = psi };

            process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg failed: {errorBuilder}");
            }

            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            return "/" + Path.GetRelativePath(webRootPath, playlistPath).Replace("\\", "/");
        }

        public async Task<TimeSpan> GetTrackDurationAsync(string inputPath)
{
    if (!File.Exists(FfprobePath))
    {
        Console.WriteLine($"[Adapter ERROR] ffprobe.exe НЕ ЗНАЙДЕНО за шляхом: {FfprobePath}");
        return TimeSpan.Zero;
    }

    var args = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputPath}\"";

    var psi = new ProcessStartInfo
    {
        FileName = FfprobePath,
        Arguments = args,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true, 
        CreateNoWindow = true
    };

    using var process = Process.Start(psi);
    if (process == null) return TimeSpan.Zero;

    var output = await process.StandardOutput.ReadToEndAsync();
    var error = await process.StandardError.ReadToEndAsync(); 
    await process.WaitForExitAsync();

    Console.WriteLine($"[Adapter] FFprobe Raw Output: '{output.Trim()}'");
    
    if (!string.IsNullOrEmpty(error))
    {
        Console.WriteLine($"[Adapter ERROR] FFprobe Error: {error}");
    }

    if (double.TryParse(output.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double seconds))
    {
        var duration = TimeSpan.FromSeconds(seconds);
        Console.WriteLine($"[Adapter] Duration parsed: {duration}");
        return duration;
    }

    Console.WriteLine("[Adapter ERROR] Не вдалося розпарсити тривалість.");
    return TimeSpan.Zero;
}
    }
}