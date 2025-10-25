using Microsoft.EntityFrameworkCore; 
using OnlineRadioStation.Domain;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;

namespace OnlineRadioStation.Data
{
    public class FavoriteRepository : RepositoryBase<FavoriteStation, Guid>, IFavoriteRepository
    {
        public FavoriteRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<IEnumerable<FavoriteStation>> GetFavoritesByUserIdAsync(Guid userId)
        {

            return await _dbSet
                .Where(fs => fs.UserId == userId)
                .Include(fs => fs.Station) 
                .ToListAsync();
        }

         public async Task<bool> IsFavoriteAsync(Guid userId, Guid stationId)
         {
             return await _dbSet.AnyAsync(fs => fs.UserId == userId && fs.StationId == stationId);
         }
    }
}