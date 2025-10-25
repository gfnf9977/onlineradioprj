using OnlineRadioStation.Domain;
using System;
using System.Collections.Generic; 
using System.Threading.Tasks;

namespace OnlineRadioStation.Data
{
    public interface IFavoriteRepository : IRepository<FavoriteStation, Guid>
    {
        Task<IEnumerable<FavoriteStation>> GetFavoritesByUserIdAsync(Guid userId);

        Task<bool> IsFavoriteAsync(Guid userId, Guid stationId);
    }
}