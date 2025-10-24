using Microsoft.EntityFrameworkCore; 
using OnlineRadioStation.Domain;
using System;
using System.Threading.Tasks; 

namespace OnlineRadioStation.Data
{
    public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
    {
        public UserRepository(ApplicationContext context) : base(context)
        {
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }
    }
}