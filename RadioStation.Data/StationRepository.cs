using OnlineRadioStation.Domain;
using System;

namespace OnlineRadioStation.Data
{

    public class StationRepository : RepositoryBase<RadioStationEntity, Guid>, IStationRepository
    {

        public StationRepository(ApplicationContext context) : base(context)
        {
        }
    }
}