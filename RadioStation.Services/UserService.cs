using Microsoft.EntityFrameworkCore;
using OnlineRadioStation.Data;
using OnlineRadioStation.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISavedStationRepository _savedRepo;
        private readonly ILikeDislikeRepository _likeRepo;

        public UserService(
            IUserRepository userRepository,
            ISavedStationRepository savedRepo,
            ILikeDislikeRepository likeRepo)
        {
            _userRepository = userRepository;
            _savedRepo = savedRepo;
            _likeRepo = likeRepo;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAll().ToListAsync();
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null) { return null; }
            if (user.Role == "Banned")
            {
                throw new Exception("Ваш акаунт заблоковано адміністратором.");
            }
            if (user.PasswordHash == password)
            {
                return user;
            }
            return null;
        }

        public async Task<User> CreateUserAsync(string username, string password, string email)
        {
            var existingUserByUsername = await _userRepository.GetUserByUsernameAsync(username);
            if (existingUserByUsername != null)
            {
                throw new Exception("Користувач з таким ім'ям вже існує.");
            }
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = username,
                PasswordHash = password,
                Email = email,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };
            _userRepository.AddEntity(newUser);
            await _userRepository.SaveChangesAsync();
            return newUser;
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userRepository.GetById(id);
        }

        public async Task UpdateUserRoleAsync(Guid id, string newRole)
        {
            var user = await _userRepository.GetById(id);
            if (user != null)
            {
                user.Role = newRole;
                _userRepository.UpdateEntity(user);
                await _userRepository.SaveChangesAsync();
            }
        }

        public async Task UpdateUserRoleAndStationAsync(Guid id, string newRole, Guid? stationId)
        {
            var user = await _userRepository.GetById(id);
            if (user != null)
            {
                user.Role = newRole;
                user.AssignedStationId = stationId;
                _userRepository.UpdateEntity(user);
                await _userRepository.SaveChangesAsync();
            }
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepository.DeleteEntity(id);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<bool> ToggleSavedStationAsync(Guid userId, Guid stationId)
        {
            var existing = await _savedRepo.GetAll()
                .FirstOrDefaultAsync(s => s.UserId == userId && s.StationId == stationId);
            if (existing != null)
            {
                await _savedRepo.DeleteEntity(existing.SavedId);
                await _savedRepo.SaveChangesAsync();
                return false;
            }
            else
            {
                var newSaved = new SavedStation
                {
                    SavedId = Guid.NewGuid(),
                    UserId = userId,
                    StationId = stationId,
                    SavedAt = DateTime.UtcNow
                };
                _savedRepo.AddEntity(newSaved);
                await _savedRepo.SaveChangesAsync();
                return true;
            }
        }

        public async Task<IEnumerable<Guid>> GetSavedStationIdsAsync(Guid userId)
        {
            return await _savedRepo.GetAll()
                .Where(s => s.UserId == userId)
                .Select(s => s.StationId)
                .ToListAsync();
        }

        public async Task<int> ToggleTrackRatingAsync(Guid userId, Guid trackId, bool isLikeBtnPressed)
        {
            var existing = await _likeRepo.GetAll()
                .FirstOrDefaultAsync(l => l.UserId == userId && l.TrackId == trackId);
            if (existing != null)
            {
                if (existing.IsLike == isLikeBtnPressed)
                {
                    await _likeRepo.DeleteEntity(existing.LikeId);
                    await _likeRepo.SaveChangesAsync();
                    return 0;
                }
                else
                {
                    existing.IsLike = isLikeBtnPressed;
                    _likeRepo.UpdateEntity(existing);
                    await _likeRepo.SaveChangesAsync();
                    return isLikeBtnPressed ? 1 : -1;
                }
            }
            else
            {
                var newRating = new LikeDislike
                {
                    LikeId = Guid.NewGuid(),
                    UserId = userId,
                    TrackId = trackId,
                    IsLike = isLikeBtnPressed,
                    CreatedAt = DateTime.UtcNow
                };
                _likeRepo.AddEntity(newRating);
                await _likeRepo.SaveChangesAsync();
                return isLikeBtnPressed ? 1 : -1;
            }
        }

        public async Task<int> GetUserTrackRatingAsync(Guid userId, Guid trackId)
        {
            var rating = await _likeRepo.GetAll()
                .FirstOrDefaultAsync(l => l.UserId == userId && l.TrackId == trackId);

            if (rating == null) return 0;
            return rating.IsLike ? 1 : -1;
        }

        public async Task<Dictionary<Guid, int>> GetUserTrackRatingsAsync(Guid userId)
        {
            return await _likeRepo.GetAll()
                .Where(l => l.UserId == userId)
                .ToDictionaryAsync(l => l.TrackId, l => l.IsLike ? 1 : -1);
        }

        public async Task<(int Likes, int Dislikes)> GetTrackVotesAsync(Guid trackId)
        {
            var votes = await _likeRepo.GetAll()
                .Where(l => l.TrackId == trackId)
                .ToListAsync(); 

            int likes = votes.Count(l => l.IsLike);
            int dislikes = votes.Count(l => !l.IsLike);

            return (likes, dislikes);
        }

        public async Task<Dictionary<Guid, (int Likes, int Dislikes)>> GetVotesForTracksAsync(IEnumerable<Guid> trackIds)
{
            var allVotes = await _likeRepo.GetAll()
               .Where(l => trackIds.Contains(l.TrackId))
               .ToListAsync();

            var result = new Dictionary<Guid, (int Likes, int Dislikes)>();

            foreach (var trackId in trackIds)
            {
                var trackVotes = allVotes.Where(v => v.TrackId == trackId);
                int likes = trackVotes.Count(v => v.IsLike);
                int dislikes = trackVotes.Count(v => !v.IsLike);
        
                result[trackId] = (likes, dislikes);
            } 

    return result;
}

    }
}
