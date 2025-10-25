using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace OnlineRadioStation.Domain
{
    public class FavoriteStation
    {
        [Key]
        public Guid FavoriteId { get; set; } 

        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!; 

        public Guid StationId { get; set; }
        [ForeignKey("StationId")]
        public RadioStationEntity Station { get; set; } = null!; 


    }
}