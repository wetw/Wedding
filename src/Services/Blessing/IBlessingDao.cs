using System.Collections.Generic;
using System.Threading.Tasks;
using Wedding.Data;

namespace Wedding.Services
{
    public interface IBlessingDao
    {
        Task<IList<Blessing>> GetListAsync(string lineId, int pageIndex = 1, int pageSize = 10);

        Task<int> Countsync(string lineId);

        Task<Blessing> AddAsync(Blessing blessing);
    }
}
