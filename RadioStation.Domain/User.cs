using System;
using System.Collections.Generic; 
using System.ComponentModel.DataAnnotations;

namespace OnlineRadioStation.Domain
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public ICollection<RadioStationEntity> CreatedStations { get; set; } = new List<RadioStationEntity>();
        public ICollection<PlaybackQueue> AddedQueues { get; set; } = new List<PlaybackQueue>();
        public ICollection<SavedStation> SavedStations { get; set; } = new List<SavedStation>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<LikeDislike> LikesDislikes { get; set; } = new List<LikeDislike>();
        public ICollection<DjStream> DjStreams { get; set; } = new List<DjStream>();
    }
}