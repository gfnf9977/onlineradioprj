using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineRadioStation.Domain;
using OnlineRadioStation.Services;
using OnlineRadioStation.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RadioStationSolution.WebApp.Controllers
{
    public class DjController : Controller
    {
        private readonly IStationService _stationService;
        private readonly IUserService _userService;
        private readonly ITrackRepository _trackRepo;
        private readonly IPlaybackQueueRepository _queueRepo;

        public DjController(
            IStationService stationService,
            IUserService userService,
            ITrackRepository trackRepo,
            IPlaybackQueueRepository queueRepo)
        {
            _stationService = stationService;
            _userService = userService;
            _trackRepo = trackRepo;
            _queueRepo = queueRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var stations = await _stationService.GetAllStationsAsync();
            ViewBag.Stations = new SelectList(stations, "StationId", "StationName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile trackFile, string title, Guid stationId, int bitrate)
        {
            if (trackFile == null || trackFile.Length == 0)
            {
                ModelState.AddModelError("trackFile", "Будь ласка, оберіть MP3-файл.");
                var stations = await _stationService.GetAllStationsAsync();
                ViewBag.Stations = new SelectList(stations, "StationId", "StationName");
                return View();
            }
            if (string.IsNullOrEmpty(title))
            {
                ModelState.AddModelError("title", "Будь ласка, введіть назву треку.");
                var stations = await _stationService.GetAllStationsAsync();
                ViewBag.Stations = new SelectList(stations, "StationId", "StationName");
                return View();
            }
            if (stationId == Guid.Empty)
            {
                ModelState.AddModelError("stationId", "Будь ласка, оберіть станцію.");
                var stations = await _stationService.GetAllStationsAsync();
                ViewBag.Stations = new SelectList(stations, "StationId", "StationName");
                return View();
            }
            if (bitrate == 0)
            {
                ModelState.AddModelError("bitrate", "Будь ласка, оберіть якість (бітрейт).");
                var stations = await _stationService.GetAllStationsAsync();
                ViewBag.Stations = new SelectList(stations, "StationId", "StationName");
                return View();
            }

            var tempFileName = $"{Guid.NewGuid()}{Path.GetExtension(trackFile.FileName)}";
            var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);
            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await trackFile.CopyToAsync(stream);
            }

            var djUser = (await _userService.GetAllUsersAsync()).FirstOrDefault();
            if (djUser == null)
            {
                ModelState.AddModelError("", "User not found");
                var stations = await _stationService.GetAllStationsAsync();
                ViewBag.Stations = new SelectList(stations, "StationId", "StationName");
                return View();
            }

            IAudioConverter adapter = new FFmpegAdapter();
            StreamFactory factory = new BitrateStreamFactory(adapter);
            IAudioProcessor manualFacade = new AudioProcessingFacade(
                adapter,
                _trackRepo,
                _queueRepo,
                factory
            );

            try
            {
                await manualFacade.ProcessNewTrackAsync(
                    tempFilePath: tempFilePath,
                    title: title,
                    stationId: stationId,
                    djId: djUser.UserId,
                    bitrate: bitrate
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ПОМИЛКА: {ex.Message}");
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                {
                    System.IO.File.Delete(tempFilePath);
                }
            }

            return RedirectToAction("Listen", "Home", new { id = stationId });
        }
    }
}
