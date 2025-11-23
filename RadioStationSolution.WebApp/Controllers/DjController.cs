using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        private async Task<User?> GetCurrentUserAsync()
        {
            var userIdStr = HttpContext.Session.GetString("CurrentUserId");
            if (string.IsNullOrEmpty(userIdStr))
            {
                return null;
            }

            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                return await _userService.GetUserByIdAsync(userId);
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var djUser = await GetCurrentUserAsync();
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

            var djUser = await GetCurrentUserAsync();
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

            return RedirectToAction(nameof(ManagePlaylist));
        }

        [HttpGet]
        public async Task<IActionResult> ManageStreams()
        {
            var djUser = await GetCurrentUserAsync();
            if (djUser == null)
                return Content("Діджея не знайдено.");

            var history = await _stationService.GetStreamsByDjAsync(djUser.UserId);
            var activeStream = await _stationService.GetActiveStreamAsync(djUser.UserId);

            ViewBag.IsLive = (activeStream != null);
            ViewBag.StationId = djUser.AssignedStationId;

            if (activeStream != null)
            {
                ViewBag.StreamStartTime = DateTime.SpecifyKind(activeStream.StartTime, DateTimeKind.Utc).ToString("o");
            }

            return View(history);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStream(string action)
        {
            var djUser = await GetCurrentUserAsync();
            if (djUser == null || djUser.AssignedStationId == null)
                return RedirectToAction("ManageStreams");

            try
            {
                if (action == "start")
                {
                    await _stationService.StartStreamAsync(djUser.AssignedStationId.Value, djUser.UserId);
                }
                else if (action == "stop")
                {
                    await _stationService.StopStreamAsync(djUser.UserId);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(ManageStreams));
        }

        [HttpGet]
        public async Task<IActionResult> ManagePlaylist()
        {
            var djUser = await GetCurrentUserAsync();
            if (djUser == null || djUser.AssignedStationId == null)
                return Content("Помилка доступу.");

            var station = await _stationService.GetStationWithPlaylistAsync(djUser.AssignedStationId.Value);
            var playlist = station.Playbacks.OrderBy(p => p.QueuePosition).ToList();

            return View(playlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromQueue(Guid queueId)
        {
            var djUser = await GetCurrentUserAsync();
            if (djUser == null)
                return RedirectToAction(nameof(ManagePlaylist));

            try
            {
                await _queueRepo.DeleteEntity(queueId);
                await _queueRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from queue: {ex.Message}");
            }

            return RedirectToAction(nameof(ManagePlaylist));
        }

        [HttpGet]
        public async Task<IActionResult> Library()
        {
            var djUser = await GetCurrentUserAsync();
            if (djUser == null)
                return RedirectToAction("Login", "Home");

            var allTracks = await _trackRepo.GetAll()
                .OrderByDescending(t => t.Title)
                .ToListAsync();

            return View(allTracks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTrackTitle(Guid trackId, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
            {
                return RedirectToAction(nameof(Library));
            }

            var track = await _trackRepo.GetById(trackId);
            if (track != null)
            {
                track.Title = newTitle;
                _trackRepo.UpdateEntity(track);
                await _trackRepo.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Library));
        }
    }
}
