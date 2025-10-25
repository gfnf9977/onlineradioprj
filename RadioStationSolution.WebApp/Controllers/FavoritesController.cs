using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; 

namespace RadioStationSolution.WebApp.Controllers
{
    // TODO: Додати [Authorize] , щоб доступ мали лише залогінені користувачі
    public class FavoritesController : Controller
    {
        private readonly IFavoriteService _favoriteService;
        private readonly IUserService _userService; 

        public FavoritesController(IFavoriteService favoriteService, IUserService userService)
        {
            _favoriteService = favoriteService;
            _userService = userService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Add(Guid stationId)
        {

            var users = await _userService.GetAllUsersAsync();
            var currentUser = users.FirstOrDefault(); 
            if (currentUser == null)
            {
                TempData["Error"] = "Помилка: Не вдалося визначити користувача.";
                return RedirectToAction("UserDashboard", "Home"); 
            }
            Guid userId = currentUser.UserId;

            try
            {
                var command = new AddFavoriteStationCommand(_favoriteService, userId, stationId);
                await command.ExecuteAsync();
                TempData["Message"] = "Станцію додано до улюблених!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Помилка додавання: {ex.Message}";
            }

            return RedirectToAction("UserDashboard", "Home");
        }

        // TODO: Додати дію для видалення з улюблених (Remove)
        // TODO: Додати сторінку для перегляду списку улюблених (Index або List)
    }
}