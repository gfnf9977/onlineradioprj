using Microsoft.EntityFrameworkCore;
using OnlineRadioStation.Data;
using OnlineRadioStation.Domain;
using System; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineRadioStation.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAll().ToListAsync();
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);
            if (user == null) { return null; }
            if (user.PasswordHash == password) { return user; }
            return null;
        }


public async Task<User> CreateUserAsync(string username, string password, string email)
{

    var existingUserByUsername = await _userRepository.GetUserByUsernameAsync(username);
    if (existingUserByUsername != null)
    {
        throw new Exception("Користувач з таким ім'ям вже існує.");
    }

    // TODO: Add GetUserByEmailAsync method to IUserRepository and UserRepository
    // var existingUserByEmail = await _userRepository.GetUserByEmailAsync(email);
    // if (existingUserByEmail != null)
    // {
    //     throw new Exception("Користувач з таким email вже існує.");
    // }

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
    }
}