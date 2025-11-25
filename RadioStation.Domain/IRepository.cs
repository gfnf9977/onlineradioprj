using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRadioStation.Domain
{
    public interface IRepository<T, TKey> where T : class
    {
        Task<T?> GetById(TKey id);
        IQueryable<T> GetAll();
        void AddEntity(T entity);
        void UpdateEntity(T entity);
        Task DeleteEntity(TKey id); 
        Task SaveChangesAsync();
    }
}