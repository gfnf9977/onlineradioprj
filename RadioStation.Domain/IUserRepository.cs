using OnlineRadioStation.Domain;
using System;
using System.Threading.Tasks; 

namespace OnlineRadioStation.Domain
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> GetUserByUsernameAsync(string username);
    }
}