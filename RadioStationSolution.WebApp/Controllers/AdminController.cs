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
            foreach (var track in allTracks)
            {
                track.Accept(visitor);
            }
            foreach (var stream in allStreams)
            {
                stream.Accept(visitor);
            }
            foreach (var item in allQueueItems)
            {
                item.Accept(visitor);
            }
            return View(visitor);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = new SelectList(new[] { "User", "Dj", "Admin" }, user.Role);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(Guid userId, string role)
        {
            try
            {
                await _userService.UpdateUserRoleAsync(userId, role);
                return RedirectToAction(nameof(ManageUsers));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Помилка: {ex.Message}");
                var user = await _userService.GetUserByIdAsync(userId);
                ViewBag.Roles = new SelectList(new[] { "User", "Dj", "Admin" }, role);
                return View(user);
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
