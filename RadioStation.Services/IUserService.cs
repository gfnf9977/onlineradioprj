using OnlineRadioStation.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<User> CreateUserAsync(string username, string password, string email);
        Task<User?> GetUserByIdAsync(Guid id); 
        Task UpdateUserRoleAsync(Guid id, string newRole); 
        Task DeleteUserAsync(Guid id); 
        Task UpdateUserRoleAndStationAsync(Guid id, string newRole, Guid? stationId);
        Task<bool> ToggleSavedStationAsync(Guid userId, Guid stationId); 
        Task<IEnumerable<Guid>> GetSavedStationIdsAsync(Guid userId); 
        Task<int> ToggleTrackRatingAsync(Guid userId, Guid trackId, bool isLikeBtnPressed);
        Task<int> GetUserTrackRatingAsync(Guid userId, Guid trackId);
        Task<Dictionary<Guid, int>> GetUserTrackRatingsAsync(Guid userId);
    }
}