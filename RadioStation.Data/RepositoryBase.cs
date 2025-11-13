using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using OnlineRadioStation.Domain;

namespace OnlineRadioStation.Data
{
    public abstract class RepositoryBase<T, TKey> : IRepository<T, TKey> where T : class
    {
        protected ApplicationContext _context;
        protected DbSet<T> _dbSet;

        public RepositoryBase(ApplicationContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetById(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.AsNoTracking();
        }

        public void AddEntity(T entity)
        {
            _dbSet.Add(entity);
        }

        public void UpdateEntity(T entity)
        {
            _dbSet.Update(entity);
        }

        public async Task DeleteEntity(TKey id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}