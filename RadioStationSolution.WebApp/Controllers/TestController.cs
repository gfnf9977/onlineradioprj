using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Domain;
using OnlineRadioStation.Services;

namespace RadioStationSolution.WebApp.Controllers
{
    public class TestController : Controller
    {
        private readonly StreamingService _streamingService;

        public TestController(StreamingService streamingService)
        {
            _streamingService = streamingService;
        }

        public IActionResult IteratorTest()
        {
            var queue = new PlaybackQueue(); 
            _streamingService.StartStreaming(queue);
            return Content("Переглянь консоль (Output → Debug)");
        }
    }
}