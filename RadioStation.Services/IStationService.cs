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
        Task<RadioStationEntity?> GetStationWithPlaylistAsync(Guid id);
        Task<IEnumerable<DjStream>> GetStreamsByDjAsync(Guid djId);
        Task StartStreamAsync(Guid stationId, Guid djId, bool isRandom = false);
        Task StopStreamAsync(Guid djId);
        Task<DjStream?> GetActiveStreamAsync(Guid djId);
        Task UpdateStreamAsync(DjStream stream);
        Task<(Track? CurrentTrack, TimeSpan Offset)> GetCurrentRadioStateAsync(Guid stationId);
        Task<List<Track>> GetCurrentPlaylistOrderAsync(Guid stationId);
    }
}