using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineRadioStation.Domain
{
    public class LikeDislike
    {
        [Key]
        public Guid LikeId { get; set; }
        public Guid UserId { get; set; }
        public Guid TrackId { get; set; }
        public bool IsLike { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; } = null!;
        public Track Track { get; set; } = null!;
    }
}