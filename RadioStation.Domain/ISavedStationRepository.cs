using System;

namespace OnlineRadioStation.Domain
{
    public interface ISavedStationRepository : IRepository<SavedStation, Guid>
    {
    }
}