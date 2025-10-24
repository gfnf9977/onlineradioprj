using OnlineRadioStation.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<User> CreateUserAsync(string username, string password, string email);
    }
}