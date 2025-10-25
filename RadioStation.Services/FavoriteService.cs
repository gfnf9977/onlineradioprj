using OnlineRadioStation.Data;
using OnlineRadioStation.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;

        public FavoriteService(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository;
        }

        public async Task AddFavoriteAsync(Guid userId, Guid stationId)
        {
            bool alreadyFavorite = await _favoriteRepository.IsFavoriteAsync(userId, stationId);
            if (alreadyFavorite)
            {

                Console.WriteLine("Станція вже є в улюблених."); 
                return;
            }

            // TODO: Додати перевірку, чи існують User та Station з такими ID

            var newFavorite = new FavoriteStation
            {
                FavoriteId = Guid.NewGuid(), 
                UserId = userId,
                StationId = stationId
            };

            _favoriteRepository.AddEntity(newFavorite);
            await _favoriteRepository.SaveChangesAsync();
        }

        public async Task RemoveFavoriteAsync(Guid userId, Guid stationId)
        {
             var favorite = await _favoriteRepository.GetAll()
                 .FirstOrDefaultAsync(fs => fs.UserId == userId && fs.StationId == stationId);

             if (favorite != null)
             {
                 _favoriteRepository.DeleteEntity(favorite.FavoriteId);
                 await _favoriteRepository.SaveChangesAsync();
             }
             else
             {
                 Console.WriteLine("Спроба видалити неіснуючу улюблену станцію.");
             }
        }


        public async Task<IEnumerable<RadioStationEntity>> GetUserFavoritesAsync(Guid userId)
        {
            var favorites = await _favoriteRepository.GetFavoritesByUserIdAsync(userId);
            return favorites.Select(f => f.Station);
        }
    }
}