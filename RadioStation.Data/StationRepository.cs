using Microsoft.EntityFrameworkCore;
using OnlineRadioStation.Domain;
using System;
using System.Threading.Tasks;

namespace OnlineRadioStation.Data
{
    public class StationRepository : RepositoryBase<RadioStationEntity, Guid>, IStationRepository
    {
        public StationRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<RadioStationEntity?> GetStationWithPlaylistAsync(Guid id)
        {
            return await _dbSet
                .Include(station => station.Playbacks)
                    .ThenInclude(playback => playback.Track)
                .FirstOrDefaultAsync(station => station.StationId == id);
        }
    }
}
