using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineRadioStation.Domain
{
    public class Review
    {
        [Key]
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public Guid TrackId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = null!;
        public Track Track { get; set; } = null!;
    }
}