using System.Collections.Generic;
using System.Threading.Tasks;
using Wedding.Data;

namespace Wedding.Services
{
    public interface IBlessingDao
    {
        Task<IEnumerable<Blessing>> GetListAsync(string lineId = null, int pageIndex = 1, int pageSize = 10);

        Task<int> CountAsync(string lineId);

        Task<Blessing> AddAsync(Blessing blessing);

        Task<Blessing> GetAsync(int id);
    }
}
