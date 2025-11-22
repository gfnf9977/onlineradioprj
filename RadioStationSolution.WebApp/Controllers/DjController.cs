using Microsoft.AspNetCore.Mvc;
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
            var djUser = (await _userService.GetAllUsersAsync()).FirstOrDefault(u => u.Role == "Dj");

            if (djUser == null)
                return Content("У системі немає Діджеїв.");

            if (djUser.AssignedStationId == null)
                return Content("Ваш акаунт ще не прив'язаний до жодної радіостанції. Зверніться до Адміністратора.");

            var station = await _stationService.GetStationByIdAsync(djUser.AssignedStationId.Value);
            ViewBag.StationName = station?.StationName ?? "Невідома станція";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile trackFile, string title)
        {
            if (trackFile == null || trackFile.Length == 0)
                ModelState.AddModelError("trackFile", "Будь ласка, оберіть MP3-файл.");

            if (string.IsNullOrEmpty(title))
                ModelState.AddModelError("title", "Будь ласка, введіть назву треку.");

            var djUser = (await _userService.GetAllUsersAsync()).FirstOrDefault(u => u.Role == "Dj");
            if (djUser == null || djUser.AssignedStationId == null)
                return Content("Помилка доступу.");

            if (!ModelState.IsValid)
            {
                var station = await _stationService.GetStationByIdAsync(djUser.AssignedStationId.Value);
                ViewBag.StationName = station?.StationName ?? "Невідома станція";
                return View();
            }

            var tempFileName = $"{Guid.NewGuid()}{Path.GetExtension(trackFile.FileName)}";
            var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

            await using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await trackFile.CopyToAsync(stream);
            }

            IAudioConverter adapter = new FFmpegAdapter();
            StreamFactory factory = new BitrateStreamFactory(adapter);
            IAudioProcessor facade = new AudioProcessingFacade(adapter, _trackRepo, _queueRepo, factory);

            try
            {
                await facade.ProcessNewTrackAsync(
                    tempFilePath: tempFilePath,
                    title: title,
                    stationId: djUser.AssignedStationId.Value,
                    djId: djUser.UserId
                );
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Помилка обробки треку: {ex.Message}");
                var station = await _stationService.GetStationByIdAsync(djUser.AssignedStationId.Value);
                ViewBag.StationName = station?.StationName ?? "Невідома станція";
                return View();
            }
            finally
            {
                if (System.IO.File.Exists(tempFilePath))
                    System.IO.File.Delete(tempFilePath);
            }

            return RedirectToAction("Listen", "Home", new { id = djUser.AssignedStationId.Value });
        }
    }
}