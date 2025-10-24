using Microsoft.EntityFrameworkCore; 
using OnlineRadioStation.Domain;
using System;
using System.Linq; 
using System.Threading.Tasks; 

namespace OnlineRadioStation.Data
{
    public class DjStreamRepository : RepositoryBase<DjStream, Guid>, IDjStreamRepository
    {
        public DjStreamRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<DjStream?> GetActiveStreamByDjIdAsync(Guid djId)
        {
            return await _dbSet
                .Where(s => s.DjId == djId && s.EndTime == null)
                .FirstOrDefaultAsync();
        }
    }
}