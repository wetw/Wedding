using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SqlSugar;
using Wedding.Data;

namespace Wedding.Services
{
    public class BlessingDao : SqlSugarBaseDao<Blessing>, IBlessingDao
    {
        public BlessingDao() : base(null) { }

        public BlessingDao(IOptions<ConnectionConfig> config) : base(config) { }

        public new Task<Blessing> AddAsync(Blessing blessing)
        {
            blessing.CreationTime = blessing.LastModifyTime = System.DateTime.UtcNow;
            return base.AddAsync(blessing);
        }

        public async Task<int> CountAsync(string lineId)
        {
            RefAsync<int> total = 0;
            _ = await _db.Queryable<Blessing>()
                .Where(x => x.LineId == lineId)
                .ToPageListAsync(1, 1, total).ConfigureAwait(false);
            return total.Value;
        }

        public Task<Blessing> GetAsync(int id) => GetAsync(x => x.Id == id);

        public async Task<IEnumerable<Blessing>> GetListAsync(
            string lineId = null,
            int pageIndex = 1,
            int pageSize = 10,
            Expression<Func<Blessing, object>> orderBy = null,
            OrderByType orderByType = OrderByType.Asc)
        {
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            var blessingQueryable = _db.Queryable<Blessing>()
                .Where(x => string.IsNullOrWhiteSpace(lineId) || x.LineId == lineId);
            if (orderBy is not null)
            {
                blessingQueryable = blessingQueryable.OrderBy(orderBy, orderByType);
            }
            RefAsync<int> total = 0;
            return pageSize == 0
                ? await blessingQueryable.ToListAsync().ConfigureAwait(false)
                : await blessingQueryable.ToPageListAsync(pageIndex, pageSize, total).ConfigureAwait(false);
        }
    }
}
