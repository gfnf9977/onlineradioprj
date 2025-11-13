using OnlineRadioStation.Domain;
using System;

namespace OnlineRadioStation.Data
{
    public class TrackRepository : RepositoryBase<Track, Guid>, ITrackRepository
    {
        public TrackRepository(ApplicationContext context) : base(context)
        {
        }
    }
}