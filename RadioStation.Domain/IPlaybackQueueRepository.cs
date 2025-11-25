using OnlineRadioStation.Domain;
using System;

namespace OnlineRadioStation.Domain
{
    public interface IPlaybackQueueRepository : IRepository<PlaybackQueue, Guid>
    {
    }
}