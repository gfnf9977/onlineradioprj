using Microsoft.EntityFrameworkCore;
using OnlineRadioStation.Domain;

namespace OnlineRadioStation.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RadioStationEntity> Stations { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<LikeDislike> LikesDislikes { get; set; }
        public DbSet<DjStream> Streams { get; set; }
        public DbSet<SavedStation> SavedStations { get; set; }
        public DbSet<PlaybackQueue> PlaybackQueues { get; set; }

        public DbSet<FavoriteStation> FavoriteStations { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

             modelBuilder.Entity<FavoriteStation>()
                 .HasIndex(fs => new { fs.UserId, fs.StationId })
                 .IsUnique();
        }
    }
}