using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineRadioStation.Domain
{
    public partial class PlaybackQueue : IStationPlaylist
    {
        public IPlaylistIterator CreateIterator()
        {
            var tracks = new List<Track>
            {
                new Track { TrackId = Guid.NewGuid(), Title = "Song 1", Duration = TimeSpan.FromMinutes(3), QueuePosition = 1 },
                new Track { TrackId = Guid.NewGuid(), Title = "Song 2", Duration = TimeSpan.FromMinutes(4), QueuePosition = 2 }
            };

            return new PlaybackQueueIterator(tracks);
        }
    }
}