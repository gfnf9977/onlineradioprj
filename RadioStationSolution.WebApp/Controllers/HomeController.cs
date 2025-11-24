using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
            try
            {
                var user = await _userService.AuthenticateUserAsync(username, password);
                if (user != null)
                {
                    HttpContext.Session.SetString("CurrentUserId", user.UserId.ToString());
                    return user.Role.ToLower() switch
                    {
                        "admin" => RedirectToAction("AdminDashboard"),
                        "dj" => RedirectToAction("DjDashboard"),
                        _ => RedirectToAction("UserDashboard")
                    };
                }
                ViewBag.Error = "Неправильний логін або пароль.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        public IActionResult AdminDashboard() => View();

        public IActionResult DjDashboard() => View();

        public async Task<IActionResult> UserDashboard()
        {
            var stations = await _stationService.GetAllStationsAsync();
            var userIdStr = HttpContext.Session.GetString("CurrentUserId");
            HashSet<Guid> savedIds = new HashSet<Guid>();
            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                var ids = await _userService.GetSavedStationIdsAsync(userId);
                savedIds = new HashSet<Guid>(ids);
            }
            var sortedStations = stations
                .OrderByDescending(s => savedIds.Contains(s.StationId))
                .ThenBy(s => s.StationName)
                .ToList();
            ViewBag.SavedStationIds = savedIds;
            return View(sortedStations);
        }

        [HttpGet]
        public async Task<IActionResult> Listen(Guid id)
        {
            var station = await _stationService.GetStationWithPlaylistAsync(id);
            if (station == null) return NotFound();
            var (currentTrack, offset) = await _stationService.GetCurrentRadioStateAsync(id);
            bool isOffline = (currentTrack == null);
            ViewBag.StartTrackId = currentTrack?.TrackId;
            ViewBag.StartOffset = offset.TotalSeconds;
            ViewBag.IsOffline = isOffline;
            var userIdStr = HttpContext.Session.GetString("CurrentUserId");
            if (Guid.TryParse(userIdStr, out Guid userId))
            {
                ViewBag.UserRatings = await _userService.GetUserTrackRatingsAsync(userId);
            }
            else
            {
                ViewBag.UserRatings = new Dictionary<Guid, int>();
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
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(Guid stationId)
        {
            var userIdStr = HttpContext.Session.GetString("CurrentUserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }
            try
            {
                bool isSaved = await _userService.ToggleSavedStationAsync(userId, stationId);
                return Ok(new { isSaved = isSaved });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateTrack(Guid trackId, bool isLike)
        {
            var userIdStr = HttpContext.Session.GetString("CurrentUserId");
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId)) return Unauthorized();
            try
            {
                int newUserStatus = await _userService.ToggleTrackRatingAsync(userId, trackId, isLike);

                var (likes, dislikes) = await _userService.GetTrackVotesAsync(trackId);
                return Ok(new {
                    status = newUserStatus,
                    likes = likes,
                    dislikes = dislikes
                });
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet]
        public async Task<IActionResult> GetRadioStatus(Guid stationId)
        {
            var station = await _stationService.GetStationWithPlaylistAsync(stationId);
            if (station == null) return NotFound();
            var (currentTrack, offset) = await _stationService.GetCurrentRadioStateAsync(stationId);
            bool isOffline = (currentTrack == null);
            var playlistForJs = new List<object>();
            if (!isOffline)
            {
                var userIdStr = HttpContext.Session.GetString("CurrentUserId");
                var userRatings = new Dictionary<Guid, int>();
                if (Guid.TryParse(userIdStr, out Guid userId))
                {
                    userRatings = await _userService.GetUserTrackRatingsAsync(userId);
                }
                var activeTracks = station.Playbacks
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.QueuePosition)
                    .ToList();
                int position = 1;
                foreach (var item in activeTracks)
                {
                    if (!string.IsNullOrEmpty(item.Track.HlsUrl))
                    {
                        int rating = userRatings.ContainsKey(item.TrackId) ? userRatings[item.TrackId] : 0;
                        var votes = _userService.GetTrackVotesAsync(item.TrackId).Result;
                        playlistForJs.Add(new {
                            trackId = item.TrackId.ToString(),
                            title = item.Track.Title,
                            url = item.Track.HlsUrl,
                            id = position,
                            userRating = rating,
                            likesCount = votes.Likes,
                            dislikesCount = votes.Dislikes
                        });
                        position++;
                    }
                }
            }
            return Ok(new {
                isOffline = isOffline,
                startTrackId = currentTrack?.TrackId,
                startOffset = offset.TotalSeconds,
                playlist = playlistForJs
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
