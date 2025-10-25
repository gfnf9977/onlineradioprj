using OnlineRadioStation.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public interface IFavoriteService
    {
        Task AddFavoriteAsync(Guid userId, Guid stationId);

        Task RemoveFavoriteAsync(Guid userId, Guid stationId);

        Task<IEnumerable<RadioStationEntity>> GetUserFavoritesAsync(Guid userId);
    }
}