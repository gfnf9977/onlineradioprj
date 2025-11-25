using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks; 

namespace RadioStationSolution.WebApp.Controllers
{
    public class AccountController : Controller
    {
        
        [HttpPost] 
        public IActionResult Logout()
        {

            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Profile()
        {

            return View(); 
        }
    }
}