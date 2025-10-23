using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Services;
using System.Diagnostics;
using System.Threading.Tasks; // Додайте цей using

namespace RadioStationSolution.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;

        public HomeController(IUserService userService)
        {
            _userService = userService;
        }

        // Метод для показу сторінки входу (GET-запит)
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Метод для обробки даних з форми входу (POST-запит)
        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Будь ласка, введіть логін та пароль.";
                return View(); // Повертаємо на сторінку входу з помилкою
            }

            var user = await _userService.AuthenticateUserAsync(username, password);

            if (user != null)
            {
                // Успішний вхід - перенаправляємо на нову сторінку Dashboard
                // TODO: Зберегти інформацію про користувача в сесії або cookie
                return RedirectToAction("Dashboard");
            }
            else
            {
                // Невдалий вхід
                ViewBag.Error = "Неправильний логін або пароль.";
                return View(); // Повертаємо на сторінку входу з помилкою
            }
        }

        // Нова сторінка для успішного входу
        public IActionResult Dashboard()
        {
            // TODO: Передати ім'я користувача на сторінку
            return View();
        }

        // Сторінка реєстрації
        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}