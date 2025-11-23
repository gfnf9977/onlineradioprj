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

        public UserService(
            IUserRepository userRepository,
            ISavedStationRepository savedRepo)
        {
            _userRepository = userRepository;
            _savedRepo = savedRepo;
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
    }
}
