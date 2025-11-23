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

            var originalPlaylist = station.Playbacks
                .Where(p => p.IsActive)
                .OrderBy(p => p.QueuePosition)
                .Select(p => p.Track)
                .ToList();

            if (!originalPlaylist.Any())
            {
                return (null, TimeSpan.Zero);
            }

            IList<Track> currentPlaylist;

            if (activeStream.IsRandom)
            {
                var random = new Random(activeStream.StreamId.GetHashCode());
                currentPlaylist = originalPlaylist.OrderBy(x => random.Next()).ToList();
            }
            else
            {
                currentPlaylist = originalPlaylist;
            }

            long totalTicks = currentPlaylist.Sum(t => t.Duration.Ticks);
            if (totalTicks == 0)
            {
                return (currentPlaylist.First(), TimeSpan.Zero);
            }

            var realTimeElapsed = DateTime.UtcNow - activeStream.StartTime;
            long currentLoopTicks = realTimeElapsed.Ticks % totalTicks;
            var loopTimeElapsed = TimeSpan.FromTicks(currentLoopTicks);

            foreach (var track in currentPlaylist)
            {
                if (loopTimeElapsed < track.Duration)
                {
                    return (track, loopTimeElapsed);
                }
                loopTimeElapsed -= track.Duration;
            }

            return (currentPlaylist.First(), TimeSpan.Zero);
        }

        public async Task<IEnumerable<DjStream>> GetStreamsByDjAsync(Guid djId)
        {
            return await _streamRepository.GetAll()
                .Where(s => s.DjId == djId)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<DjStream?> GetActiveStreamAsync(Guid djId)
        {
            return await _streamRepository.GetAll()
                .FirstOrDefaultAsync(s => s.DjId == djId && s.EndTime == null);
        }

        public async Task StartStreamAsync(Guid stationId, Guid djId)
        {
            var active = await GetActiveStreamAsync(djId);
            if (active != null)
            {
                throw new Exception("Ефір вже триває! Спочатку завершіть поточний.");
            }

            var newStream = new DjStream
            {
                StreamId = Guid.NewGuid(),
                StationId = stationId,
                DjId = djId,
                StartTime = DateTime.UtcNow,
                EndTime = null
            };

            _streamRepository.AddEntity(newStream);
            await _streamRepository.SaveChangesAsync();
        }

        public async Task StopStreamAsync(Guid djId)
        {
            var activeStream = await GetActiveStreamAsync(djId);
            if (activeStream == null)
            {
                throw new Exception("Немає активного ефіру для завершення.");
            }

            activeStream.EndTime = DateTime.UtcNow;
            _streamRepository.UpdateEntity(activeStream);
            await _streamRepository.SaveChangesAsync();
        }

        public async Task UpdateStreamAsync(DjStream stream)
        {
            _streamRepository.UpdateEntity(stream);
            await _streamRepository.SaveChangesAsync();
        }
        
    }
}
