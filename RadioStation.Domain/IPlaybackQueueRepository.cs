using OnlineRadioStation.Domain;
using System;

namespace OnlineRadioStation.Domain
{
    public interface IPlaybackQueueRepository : IRepository<PlaybackQueue, Guid>
    {
        // Поки що нам не потрібні спеціальні методи
    }
}