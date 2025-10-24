using Microsoft.EntityFrameworkCore;
using OnlineRadioStation.Data;
using OnlineRadioStation.Domain;
using System; 
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _stationRepository;
        private readonly IUserRepository _userRepository;

        public StationService(IStationRepository stationRepository, IUserRepository userRepository)
        {
            _stationRepository = stationRepository;
            _userRepository = userRepository; 
        }

        public async Task<IEnumerable<RadioStationEntity>> GetAllStationsAsync()
        {
            return await _stationRepository.GetAll().ToListAsync();
        }

        public async Task<RadioStationEntity?> GetStationByIdAsync(Guid id)
        {
            return await _stationRepository.GetById(id);
        }

        public async Task<RadioStationEntity> AddStationAsync(string name, string description, Guid createdById)
        {
            var userExists = await _userRepository.GetById(createdById);
            if (userExists == null)
            {
                throw new Exception("Користувача, який створює станцію, не знайдено.");
            }

            var newStation = new RadioStationEntity
            {
                StationId = Guid.NewGuid(),
                StationName = name,
                Description = description,
                CreatedById = createdById
            };
            _stationRepository.AddEntity(newStation);
            await _stationRepository.SaveChangesAsync();
            return newStation;
        }

        public async Task UpdateStationAsync(Guid id, string name, string description)
        {
            var station = await _stationRepository.GetById(id);
            if (station == null)
            {
                throw new Exception("Станцію не знайдено.");
            }
            station.StationName = name;
            station.Description = description;
            _stationRepository.UpdateEntity(station);
            await _stationRepository.SaveChangesAsync();
        }

        public async Task DeleteStationAsync(Guid id)
        {
            var station = await _stationRepository.GetById(id);
            if (station == null)
            {

                return; 
            }
            _stationRepository.DeleteEntity(id); 
            await _stationRepository.SaveChangesAsync();
        }
    }
}