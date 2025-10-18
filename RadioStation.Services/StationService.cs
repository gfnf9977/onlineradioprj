using Microsoft.EntityFrameworkCore;
using OnlineRadioStation.Data; // 
using OnlineRadioStation.Domain; // 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services // 
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _stationRepository;

        public StationService(IStationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        public async Task<IEnumerable<RadioStationEntity>> GetAllStationsAsync()
        {
            return await _stationRepository.GetAll().ToListAsync();
        }
    }
}