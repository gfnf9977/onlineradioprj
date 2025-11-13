using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineRadioStation.Domain;
using OnlineRadioStation.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RadioStationSolution.WebApp.Controllers
{
    public class DjController : Controller
    {
        private readonly IAudioProcessor _audioProcessor;
        private readonly IStationService _stationService;
        private readonly IUserService _userService;

        public DjController(
            IAudioProcessor audioProcessor,
            IStationService stationService,
            IUserService userService)
        {
            _audioProcessor = audioProcessor;
            _stationService = stationService;
            _userService = userService;
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
            }
            if (string.IsNullOrEmpty(title))
            {
                ModelState.AddModelError("title", "Будь ласка, введіть назву треку.");
            }
            if (stationId == Guid.Empty)
            {
                ModelState.AddModelError("stationId", "Будь ласка, оберіть станцію.");
            }
            if (bitrate == 0)
            {
                ModelState.AddModelError("bitrate", "Будь ласка, оберіть якість (бітрейт).");
            }

            if (!ModelState.IsValid)
            {
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
                return View();
            }

            try
            {
                await _audioProcessor.ProcessNewTrackAsync(
                    tempFilePath: tempFilePath,
                    title: title,
                    stationId: stationId,
                    djId: djUser.UserId,
                    bitrate: bitrate
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ПОМИЛКА ФАСАДУ: {ex.Message}");
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
