using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineRadioStation.Domain
{
    public class SavedStation
    {
        [Key]
        public Guid SavedId { get; set; }
        public Guid UserId { get; set; }
        public Guid StationId { get; set; }
        public DateTime SavedAt { get; set; }
        public User User { get; set; } = null!;
        public RadioStationEntity Station { get; set; } = null!;
    }
}