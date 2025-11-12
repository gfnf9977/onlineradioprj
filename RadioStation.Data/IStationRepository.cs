using OnlineRadioStation.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRadioStation.Data
{

    public interface IStationRepository : IRepository<RadioStationEntity, Guid>
    {
        Task<RadioStationEntity?> GetStationWithPlaylistAsync(Guid id);
    }
}