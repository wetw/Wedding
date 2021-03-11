using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Wedding.Services
{
    public interface IBaseDao<T>
    {
        Task<T> GetAsync(Expression<Func<T, bool>> id);

        Task<T> AddAsync(T obj);

        Task<T> UpdateAsync(T obj, Expression<Func<T, bool>> where);

        Task<int> RemoveAsync(T obj);
    }
}