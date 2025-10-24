using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Services;
using System;
using System.Threading.Tasks;

namespace RadioStationSolution.WebApp.Controllers
{
    public class DjController : Controller
    {
        private readonly IDjStreamService _djStreamService;
        private readonly IUserService _userService; 

        private static Guid? _currentStreamId;
         private static Guid _mockStationId = Guid.NewGuid(); 

        public DjController(IDjStreamService djStreamService, IUserService userService)
        {
            _djStreamService = djStreamService;
            _userService = userService;
        }

         private async Task<Guid> GetCurrentStreamIdAsync()
         {
             if (_currentStreamId.HasValue) return _currentStreamId.Value;

             var users = await _userService.GetAllUsersAsync();
             var djId = users.FirstOrDefault(u => u.Role == "DJ")?.UserId ?? users.FirstOrDefault()?.UserId ?? Guid.Empty;
             if (djId == Guid.Empty) throw new Exception("Не знайдено користувача DJ.");

             var stream = await _djStreamService.GetOrCreateCurrentStreamAsync(djId, _mockStationId);
             _currentStreamId = stream?.StreamId;
             if (!_currentStreamId.HasValue) throw new Exception("Не вдалося створити/отримати стрім.");

             return _currentStreamId.Value;
         }


        [HttpPost]
        public async Task<IActionResult> Start()
        {
            try
            {
                var streamId = await GetCurrentStreamIdAsync();
                await _djStreamService.StartStreamAsync(streamId);
                TempData["Message"] = "Стрім запущено!";
            }
            catch (Exception ex) { TempData["Error"] = $"Помилка запуску: {ex.Message}"; }
            return RedirectToAction("DjDashboard", "Home"); 
        }

        [HttpPost]
        public async Task<IActionResult> Pause()
        {
             try
             {
                 var streamId = await GetCurrentStreamIdAsync();
                 await _djStreamService.PauseStreamAsync(streamId);
                 TempData["Message"] = "Стрім на паузі.";
             }
             catch (Exception ex) { TempData["Error"] = $"Помилка паузи: {ex.Message}"; }
             return RedirectToAction("DjDashboard", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Resume()
        {
              try
              {
                 var streamId = await GetCurrentStreamIdAsync();
                 await _djStreamService.ResumeStreamAsync(streamId);
                 TempData["Message"] = "Стрім відновлено.";
             }
             catch (Exception ex) { TempData["Error"] = $"Помилка відновлення: {ex.Message}"; }
             return RedirectToAction("DjDashboard", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Stop()
        {
              try
              {
                 var streamId = await GetCurrentStreamIdAsync();
                 await _djStreamService.StopStreamAsync(streamId);
                 _currentStreamId = null; 
                 TempData["Message"] = "Стрім зупинено.";
             }
             catch (Exception ex) { TempData["Error"] = $"Помилка зупинки: {ex.Message}"; }
             return RedirectToAction("DjDashboard", "Home");
        }
    }
}