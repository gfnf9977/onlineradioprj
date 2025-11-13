using OnlineRadioStation.Domain;
using System;

namespace OnlineRadioStation.Data
{
    public class PlaybackQueueRepository : RepositoryBase<PlaybackQueue, Guid>, IPlaybackQueueRepository
    {
        public PlaybackQueueRepository(ApplicationContext context) : base(context)
        {
        }
    }
}