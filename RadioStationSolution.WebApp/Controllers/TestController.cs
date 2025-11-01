using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Domain;
using OnlineRadioStation.Services;
using System.IO;
using System.Threading.Tasks;

namespace RadioStationSolution.WebApp.Controllers
{
    [Route("test")]
    public class TestController : Controller
    {
        private readonly StreamingService _streamingService;
        private readonly string _webRootPath;

        public TestController(StreamingService streamingService)
        {
            _streamingService = streamingService;
            _webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        [HttpGet("iterator")]
        public IActionResult IteratorTest()
        {
            var queue = new PlaybackQueue();
            _streamingService.StartStreaming(queue);


            return new ContentResult
            {
                Content = "<h3 style='color: green;'>ЛР4: Iterator — УСПІХ!</h3>" +
                          "<p>Переглянь консоль (Output → Debug)</p>",
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
    }
}