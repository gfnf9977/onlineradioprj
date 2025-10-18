using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineRadioStation.Data
{
    public interface IRepository<T, TKey> where T : class
    {
        Task<T?> GetById(TKey id);
        IQueryable<T> GetAll();
        void AddEntity(T entity);
        void UpdateEntity(T entity);
        void DeleteEntity(TKey id);
        Task SaveChangesAsync();
    }
}