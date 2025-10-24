using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks; 

namespace RadioStationSolution.WebApp.Controllers
{
    public class AccountController : Controller
    {
        
        [HttpPost] 
        public IActionResult Logout()
        {
            // TODO: Очистити сесію або cookie, де зберігається інформація про користувача
            // Наприклад, якщо використовуєте сесії:
            // HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Profile()
        {
            // TODO: Завантажити дані поточного користувача і передати у View
            return View(); // Потрібно буде створити Views/Account/Profile.cshtml
        }
    }
}