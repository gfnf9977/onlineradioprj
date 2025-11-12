using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Domain;
using OnlineRadioStation.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RadioStationSolution.WebApp.Controllers
{
    [Route("test")]
    public class TestController : Controller
    {
        private readonly StreamingService _streamingService;
        private readonly string _webRootPath;
        private readonly IStationService _stationService;

        public TestController(StreamingService streamingService, IStationService stationService)
        {
            _streamingService = streamingService;
            _stationService = stationService;
            _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        [HttpGet("iterator")]
        public async Task<IActionResult> IteratorTest()
        {
            var anyStation = (await _stationService.GetAllStationsAsync()).FirstOrDefault();
            if (anyStation == null)
            {
                return new ContentResult
                {
                    Content = "<h3 style='color: red;'>ЛР4: Iterator — ПОМИЛКА!</h3>" +
                              "<p>У базі даних немає жодної станції для тесту.</p>" +
                              "<p>Зайдіть в Адмін-панель і створіть станцію.</p>",
                    ContentType = "text/html; charset=utf-8"
                };
            }
            var stationWithPlaylist = await _stationService.GetStationWithPlaylistAsync(anyStation.StationId);
            _streamingService.StartStreaming(stationWithPlaylist!);
            return new ContentResult
            {
                Content = "<h3 style='color: green;'>ЛР4: Iterator — УСПІХ!</h3>" +
                          $"<p>Тест проведено на реальній станції: <b>{stationWithPlaylist!.StationName}</b></p>" +
                          "<p>Переглянь консоль (Output → Debug), щоб побачити її плейлист.</p>",
                ContentType = "text/html; charset=utf-8"
            };
        }

        [HttpGet("adapter")]
        public async Task<IActionResult> AdapterTest()
        {
            var mp3Path = Path.Combine(_webRootPath, "samples", "demo.mp3");
            if (!System.IO.File.Exists(mp3Path))
            {
                return new ContentResult
                {
                    Content = "<p style='color: red; font-weight: bold;'>ПОМИЛКА:</p>" +
                              "<p>Файл <code>wwwroot/samples/demo.mp3</code> не знайдено!</p>" +
                              "<p>Додай будь-який .mp3 файл (5–10 сек).</p>",
                    ContentType = "text/html; charset=utf-8"
                };
            }
            try
            {
                var hlsUrl = await _streamingService.CreateHlsStreamAsync(mp3Path, 128);
                var webPath = "/" + Path.GetRelativePath(_webRootPath, hlsUrl).Replace("\\", "/");
                return new ContentResult
                {
                    Content = "<div style='font-family: Arial; padding: 20px; background: #f0f8ff; border-radius: 10px;'>" +
                              "<h3 style='color: #006400;'>ЛР5: Adapter (FFmpeg) — УСПІХ!</h3>" +
                              "<p><strong>HLS стрім створено:</strong></p>" +
                              $"<p><a href='{webPath}' target='_blank' style='font-size: 18px; color: #0000ff; text-decoration: underline;'>Відтворити stream.m3u8</a></p>" +
                              "<p><small>Відкрий у <strong>VLC</strong> або браузері</small></p>" +
                              "<hr><p><em>Файли: <code>wwwroot/streams/demo/</code></em></p>" +
                              "</div>",
                    ContentType = "text/html; charset=utf-8"
                };
            }
            catch (System.Exception ex)
            {
                return new ContentResult
                {
                    Content = $"<p style='color: red;'>Помилка FFmpeg: {System.Web.HttpUtility.HtmlEncode(ex.Message)}</p>",
                    ContentType = "text/html; charset=utf-8"
                };
            }
        }

        [HttpGet("factory")]
        public IActionResult FactoryTest()
        {
            var results = new[]
            {
                _streamingService.StartStreamWithFactory(64, "Song A"),
                _streamingService.StartStreamWithFactory(128, "Song B"),
                _streamingService.StartStreamWithFactory(300, "Song C")
            };
            var html = "<h3 style='color: green;'>Factory Method:</h3>" +
                       "<ul>" +
                       string.Join("", results.Select(r => $"<li>{r}</li>")) +
                       "</ul>";
            return Content(html, "text/html; charset=utf-8");
        }

        [HttpGet("facade")]
        public IActionResult FacadeTest()
        {
            var input = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "samples", "demo.mp3");
            if (!System.IO.File.Exists(input))
                return Content("<p style='color:red'>Додай demo.mp3 у wwwroot/samples/</p>", "text/html");
            var result = _streamingService.PrepareTrack(input);
            return Content(
                $"<h3 style='color:green'>ЛР7: Facade — УСПІХ!</h3>" +
                $"<p>Оброблено: <code>{Path.GetFileName(result)}</code></p>" +
                "<p>Переглянь консоль (Output → Debug)</p>",
                "text/html; charset=utf-8"
            );
        }

        [HttpGet("visitor")]
        public IActionResult VisitorTest()
        {
            var track = new Track { Title = "Song 1", Duration = TimeSpan.FromMinutes(3) };
            var stream = new DjStream { StreamId = Guid.NewGuid(), StartTime = DateTime.Now, EndTime = DateTime.Now.AddMinutes(30) };
            var queue = new PlaybackQueue { QueueId = Guid.NewGuid(), QueuePosition = 1 };
            _streamingService.CollectStats(track, stream, queue);
            return Content(
                "<h3 style='color: green;'>ЛР8: Visitor — УСПІХ!</h3>" +
                "<p>Переглянь консоль (Output → Debug)</p>",
                "text/html; charset=utf-8"
            );
        }
    }
}
