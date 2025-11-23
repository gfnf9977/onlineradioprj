using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineRadioStation.Domain
{
    public class DjStream
    {
        [Key]
        public Guid StreamId { get; set; }
        public Guid StationId { get; set; }
        public Guid DjId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public RadioStationEntity Station { get; set; } = null!;
        public User Dj { get; set; } = null!;
        public bool IsRandom { get; set; }
        public void Accept(IStatsVisitor visitor)
        {
            visitor.VisitStream(this);
        }
    }
}
