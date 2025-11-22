using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Services;
using System;
using System.Threading.Tasks;

namespace RadioStationSolution.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly IStationService _stationService;

        public HomeController(IUserService userService, IStationService stationService)
        {
            _userService = userService;
            _stationService = stationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stations = await _stationService.GetAllStationsAsync();
            return View(stations);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Будь ласка, введіть логін та пароль.";
                return View();
            }
            var user = await _userService.AuthenticateUserAsync(username, password);
            if (user != null)
            {
                switch (user.Role.ToLower())
                {
                    case "admin":
                        return RedirectToAction("AdminDashboard");
                    case "dj":
                        return RedirectToAction("DjDashboard");
                    case "user":
                    default:
                        return RedirectToAction("UserDashboard");
                }
            }
            else
            {
                ViewBag.Error = "Неправильний логін або пароль.";
                return View();
            }
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult DjDashboard()
        {
            return View();
        }

        public async Task<IActionResult> UserDashboard()
        {
            var stations = await _stationService.GetAllStationsAsync();
            return View(stations);
        }

        [HttpGet]
        public async Task<IActionResult> Listen(Guid id)
        {
            var station = await _stationService.GetStationWithPlaylistAsync(id);
            if (station == null)
            {
                return NotFound();
            }
            return View(station);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Будь ласка, заповніть усі поля.";
                return View();
            }
            try
            {
                await _userService.CreateUserAsync(username, password, email);
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
