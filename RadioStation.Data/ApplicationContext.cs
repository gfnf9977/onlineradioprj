using Microsoft.EntityFrameworkCore;
using OnlineRadioStation.Domain;

namespace OnlineRadioStation.Data
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RadioStationEntity> Stations { get; set; }
        public DbSet<Track> Tracks { get; set; }
        public DbSet<LikeDislike> LikesDislikes { get; set; }
        public DbSet<DjStream> Streams { get; set; }
        public DbSet<SavedStation> SavedStations { get; set; }
        public DbSet<PlaybackQueue> PlaybackQueue { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<RadioStationEntity>().ToTable("Stations");
            modelBuilder.Entity<Track>().ToTable("Tracks");
            modelBuilder.Entity<LikeDislike>().ToTable("LikesDislikes");
            modelBuilder.Entity<DjStream>().ToTable("Streams");
            modelBuilder.Entity<SavedStation>().ToTable("SavedStations");
            modelBuilder.Entity<PlaybackQueue>().ToTable("PlaybackQueue");
        }
    }
}