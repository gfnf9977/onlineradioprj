using OnlineRadioStation.Domain; // 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services // 
{
    public interface IStationService
    {
        Task<IEnumerable<RadioStationEntity>> GetAllStationsAsync();
    }
}