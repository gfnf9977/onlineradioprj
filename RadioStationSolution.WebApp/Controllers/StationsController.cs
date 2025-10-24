using Microsoft.AspNetCore.Mvc;
using OnlineRadioStation.Services; 
using System;
using System.Threading.Tasks;

namespace RadioStationSolution.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly IStationService _stationService;

        public StationsController(IStationService stationService)
        {
            _stationService = stationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStations()
        {
            try
            {
                var stations = await _stationService.GetAllStationsAsync();
                return Ok(stations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}