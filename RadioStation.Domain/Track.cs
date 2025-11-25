using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnlineRadioStation.Domain
{
    public class Track
    {
        [Key]
        public Guid TrackId { get; set; }
        public string Title { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public Guid UploadedById { get; set; }
        public string HlsUrl { get; set; } = string.Empty;
        public User UploadedBy { get; set; } = null!;
        public ICollection<PlaybackQueue> Queues { get; set; } = new List<PlaybackQueue>();
        public ICollection<LikeDislike> LikesDislikes { get; set; } = new List<LikeDislike>();
        public void Accept(IStatsVisitor visitor)
        {
            visitor.VisitTrack(this);
        }
    }
}
