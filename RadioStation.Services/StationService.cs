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
        private readonly IDjStreamRepository _streamRepository;

        public StationService(
            IStationRepository stationRepository,
            IUserRepository userRepository,
            IDjStreamRepository streamRepository)
        {
            _stationRepository = stationRepository;
            _userRepository = userRepository;
            _streamRepository = streamRepository;
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
            await _stationRepository.DeleteEntity(id);
            await _stationRepository.SaveChangesAsync();
        }

        public async Task<RadioStationEntity?> GetStationWithPlaylistAsync(Guid id)
        {
            return await _stationRepository.GetStationWithPlaylistAsync(id);
        }

        public async Task<(Track? CurrentTrack, TimeSpan Offset)> GetCurrentRadioStateAsync(Guid stationId)
        {
            var allStreams = await _streamRepository.GetAll().ToListAsync();
            var activeStream = allStreams.FirstOrDefault(s => s.StationId == stationId && s.EndTime == null);
            if (activeStream == null)
            {
                return (null, TimeSpan.Zero);
            }

            var station = await GetStationWithPlaylistAsync(stationId);
            if (station == null || !station.Playbacks.Any())
            {
                return (null, TimeSpan.Zero);
            }

            var playlist = station.Playbacks
                .OrderBy(p => p.QueuePosition)
                .Select(p => p.Track)
                .ToList();

            if (!playlist.Any())
            {
                return (null, TimeSpan.Zero);
            }

            // 1. Рахуємо загальну тривалість плейлиста
            long totalTicks = playlist.Sum(t => t.Duration.Ticks);
            if (totalTicks == 0)
            {
                return (playlist.First(), TimeSpan.Zero);
            }

            var totalDuration = TimeSpan.FromTicks(totalTicks);

            // 2. Скільки часу пройшло реально
            var realTimeElapsed = DateTime.UtcNow - activeStream.StartTime;

            // 3. Використовуємо "Остачу від ділення" (Modulo), щоб зациклити час
            long currentLoopTicks = realTimeElapsed.Ticks % totalTicks;
            var loopTimeElapsed = TimeSpan.FromTicks(currentLoopTicks);

            // 4. Шукаємо трек у цьому колі
            foreach (var track in playlist)
            {
                if (loopTimeElapsed < track.Duration)
                {
                    return (track, loopTimeElapsed);
                }
                loopTimeElapsed -= track.Duration;
            }

            return (playlist.First(), TimeSpan.Zero);
        }
    }
}
