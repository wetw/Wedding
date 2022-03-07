using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SqlSugar;
using Wedding.Data;

namespace Wedding.Services
{
    public interface IBlessingDao
    {
        Task<IEnumerable<Blessing>> GetListAsync(
            string lineId = null,
            int pageIndex = 1,
            int pageSize = 10,
            Expression<Func<Blessing, object>> orderBy = null,
            OrderByType orderByType = OrderByType.Asc);

        Task<int> CountAsync(string lineId);

        Task<Blessing> AddAsync(Blessing blessing);

        Task<Blessing> GetAsync(int id);
    }
}
