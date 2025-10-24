using OnlineRadioStation.Domain;
using System; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public interface IStationService
    {
        Task<IEnumerable<RadioStationEntity>> GetAllStationsAsync();
        Task<RadioStationEntity?> GetStationByIdAsync(Guid id); 
        Task<RadioStationEntity> AddStationAsync(string name, string description, Guid createdById); 
        Task UpdateStationAsync(Guid id, string name, string description); 
        Task DeleteStationAsync(Guid id); 
    }
}