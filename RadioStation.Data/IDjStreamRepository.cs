using OnlineRadioStation.Domain;
using System;
using System.Threading.Tasks; 

namespace OnlineRadioStation.Data
{
    public interface IDjStreamRepository : IRepository<DjStream, Guid>
    {
        Task<DjStream?> GetActiveStreamByDjIdAsync(Guid djId);
    }
}