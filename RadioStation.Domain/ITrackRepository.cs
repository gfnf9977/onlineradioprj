using System;

namespace OnlineRadioStation.Domain
{
    // Тут НЕ МАЄ бути визначення 'IRepository'
    public interface ITrackRepository : IRepository<Track, Guid>
    {
        // Поки що нам не потрібні спеціальні методи
    }
}