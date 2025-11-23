using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineRadioStation.Domain
{
    public partial class PlaybackQueue
    {
        [Key]
        public Guid QueueId { get; set; }
        public Guid TrackId { get; set; }
        public Guid StationId { get; set; }
        public Guid AddedById { get; set; }
        public int QueuePosition { get; set; }
        public bool IsActive { get; set; } = true;
        public Track Track { get; set; } = null!;
        public RadioStationEntity Station { get; set; } = null!;
        public User AddedBy { get; set; } = null!;

        public void Accept(IStatsVisitor visitor)
        {
            visitor.VisitQueue(this);
        }
    }
}
