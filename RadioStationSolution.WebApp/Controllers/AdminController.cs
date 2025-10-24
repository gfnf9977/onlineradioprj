using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Services;
using System;
using System.Threading.Tasks;
using OnlineRadioStation.Domain; 

namespace RadioStationSolution.WebApp.Controllers
{
    // TODO: Додати [Authorize(Roles = "Admin")] для захисту
    public class AdminController : Controller
    {
        private readonly IStationService _stationService;
        private readonly IUserService _userService;

        public AdminController(IStationService stationService, IUserService userService)
        {
            _stationService = stationService;
            _userService = userService;
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
    }
}