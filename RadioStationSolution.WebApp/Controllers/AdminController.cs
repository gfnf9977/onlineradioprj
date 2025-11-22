using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Services;
using System;
using System.Threading.Tasks;
using OnlineRadioStation.Domain;
using OnlineRadioStation.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;

namespace RadioStationSolution.WebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IStationService _stationService;
        private readonly IUserService _userService;
        private readonly ITrackRepository _trackRepo;
        private readonly IDjStreamRepository _streamRepo;
        private readonly IPlaybackQueueRepository _queueRepo;

        public AdminController(
            IStationService stationService,
            IUserService userService,
            ITrackRepository trackRepo,
            IDjStreamRepository streamRepo,
            IPlaybackQueueRepository queueRepo)
        {
            _stationService = stationService;
            _userService = userService;
            _trackRepo = trackRepo;
            _streamRepo = streamRepo;
            _queueRepo = queueRepo;
        }

        public async Task<IActionResult> ManageStations()
        {
            var stations = await _stationService.GetAllStationsAsync();
            return View(stations);
        }

        public IActionResult AddStation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStation(string stationName, string description)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var users = await _userService.GetAllUsersAsync();
                    var adminId = users.FirstOrDefault()?.UserId ?? Guid.Empty;
                    if (adminId == Guid.Empty) throw new Exception("Не вдалося визначити ID адміністратора.");
                    await _stationService.AddStationAsync(stationName, description, adminId);
                    return RedirectToAction(nameof(ManageStations));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Помилка додавання: {ex.Message}");
                }
            }
            return View();
        }

        public async Task<IActionResult> EditStation(Guid id)
        {
            var station = await _stationService.GetStationByIdAsync(id);
            if (station == null)
            {
                return NotFound();
            }
            return View(station);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStation(Guid id, string stationName, string description)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _stationService.UpdateStationAsync(id, stationName, description);
                    return RedirectToAction(nameof(ManageStations));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Помилка оновлення: {ex.Message}");
                }
            }
            var stationToEdit = await _stationService.GetStationByIdAsync(id);
            if (stationToEdit == null) return NotFound();
            return View(stationToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStation(Guid id)
        {
            try
            {
                await _stationService.DeleteStationAsync(id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка видалення: {ex.Message}";
            }
            return RedirectToAction(nameof(ManageStations));
        }

        [HttpGet]
        public async Task<IActionResult> ShowStats()
        {
            var visitor = new ListeningStatsVisitor();
            var allTracks = await _trackRepo.GetAll().ToListAsync();
            var allStreams = await _streamRepo.GetAll().ToListAsync();
            var allQueueItems = await _queueRepo.GetAll().ToListAsync();

            foreach (var track in allTracks) track.Accept(visitor);
            foreach (var stream in allStreams) stream.Accept(visitor);
            foreach (var item in allQueueItems) item.Accept(visitor);

            return View(visitor);
        }

        [HttpPost]
        public async Task<IActionResult> CleanOrphanFiles()
        {
            var allTracks = await _trackRepo.GetAll().ToListAsync();

            var validFolderNames = allTracks
                .Where(t => !string.IsNullOrEmpty(t.HlsUrl))
                .Select(t =>
                {
                    var parts = t.HlsUrl.Split('/');
                    return parts.Length >= 3 ? parts[^2] : null;
                })
                .Where(n => n != null)
                .ToHashSet();

            var streamsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "streams");
            if (!Directory.Exists(streamsPath))
                return RedirectToAction("ShowStats");

            var physicalDirectories = Directory.GetDirectories(streamsPath);
            int deletedCount = 0;

            foreach (var dirPath in physicalDirectories)
            {
                var dirName = Path.GetFileName(dirPath);
                if (!validFolderNames.Contains(dirName))
                {
                    try
                    {
                        Directory.Delete(dirPath, true);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Не вдалося видалити папку {dirName}: {ex.Message}");
                    }
                }
            }

            TempData["Message"] = $"Очищення завершено. Видалено папок-сиріт: {deletedCount}";
            return RedirectToAction("ShowStats");
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            var sortedUsers = users.OrderBy(u =>
            {
                return u.Role.ToLower() switch
                {
                    "admin" => 1,
                    "dj"    => 2,
                    "user"  => 3,
                    "banned"=> 4,
                    _       => 5
                };
            })
            .ThenBy(u => u.Username);

            return View(sortedUsers);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            if (user.Role == "Admin")
            {
                return Content("Не можна редагувати Адміна.");
            }

            var allowedRoles = new[] { "User", "Dj", "Banned" };
            ViewBag.Roles = new SelectList(allowedRoles, user.Role);

            var stations = await _stationService.GetAllStationsAsync();
            ViewBag.Stations = new SelectList(stations, "StationId", "StationName", user.AssignedStationId);

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(Guid userId, string role, Guid? assignedStationId)
        {
            try
            {
                await _userService.UpdateUserRoleAndStationAsync(userId, role, assignedStationId);
                return RedirectToAction(nameof(ManageUsers));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка: {ex.Message}";
                return RedirectToAction(nameof(ManageUsers));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
            }
            catch(Exception ex)
            {
                TempData["Error"] = $"Помилка видалення: {ex.Message}";
            }
            return RedirectToAction(nameof(ManageUsers));
        }
    }
}