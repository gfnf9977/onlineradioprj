using OnlineRadioStation.Domain;
using System;

namespace OnlineRadioStation.Data
{
    public class SavedStationRepository : RepositoryBase<SavedStation, Guid>, ISavedStationRepository
    {
        public SavedStationRepository(ApplicationContext context) : base(context)
        {
        }
    }
}