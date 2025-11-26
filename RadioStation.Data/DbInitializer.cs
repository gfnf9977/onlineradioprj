using BCrypt.Net;
using OnlineRadioStation.Domain;
using System;
using System.Linq;

namespace OnlineRadioStation.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationContext context)
        {

            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return; 
            }
            var admin = new User
            {
                UserId = Guid.NewGuid(),
                Username = "admin",
                Email = "admin@radio.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(admin);

            var dj = new User
            {
                UserId = Guid.NewGuid(),
                Username = "dj",
                Email = "dj@radio.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("dj"), 
                Role = "Dj",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(dj);

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "user",
                Email = "user@radio.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user"),
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);

            var roks = new RadioStationEntity
            {
                StationId = Guid.NewGuid(),
                StationName = "Radio ROKS",
                Description = "Тільки рок! Найкращі хіти.",
                CreatedById = admin.UserId
            };
            context.Stations.Add(roks);

            var hitfm = new RadioStationEntity
            {
                StationId = Guid.NewGuid(),
                StationName = "Hit FM",
                Description = "Тільки хіти 90-х та сучасності.",
                CreatedById = admin.UserId
            };
            context.Stations.Add(hitfm);

            dj.AssignedStationId = roks.StationId;

            context.SaveChanges();
        }
    }
}