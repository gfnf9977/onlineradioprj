using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OnlineRadioStation.Domain
{
    public class RadioStationEntity : IStationPlaylist
    {
        [Key]
        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;
        public ICollection<PlaybackQueue> Playbacks { get; set; } = new List<PlaybackQueue>();
        public ICollection<SavedStation> SavedByUsers { get; set; } = new List<SavedStation>();
        public ICollection<DjStream> DjStreams { get; set; } = new List<DjStream>();

        public IPlaylistIterator CreateIterator()
        {
            var tracks = this.Playbacks
                .OrderBy(p => p.QueuePosition)
                .Select(p => p.Track)
                .ToList();
            return new PlaybackQueueIterator(tracks);
        }
    }
}
