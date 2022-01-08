using System.Collections.Generic;
using System.Threading.Tasks;
using SqlSugar;
using Wedding.Data;

namespace Wedding.Services
{
    public class BlessingDao : SqlSugarBaseDao<Blessing>, IBlessingDao
    {
        public BlessingDao() : base(null) { }

        public BlessingDao(ConnectionConfig config) : base(config) { }

        public new Task<Blessing> AddAsync(Blessing blessing)
        {
            blessing.CreationTime = blessing.LastModifyTime = System.DateTime.UtcNow;
            return base.AddAsync(blessing);
        }

        public async Task<int> Countsync(string lineId)
        {
            RefAsync<int> total = 0;
            _ = await _db.Queryable<Blessing>()
                .Where(x => x.LineId == lineId)
                .ToPageListAsync(1, 1, total).ConfigureAwait(false);
            return total.Value;
        }

        public async Task<IList<Blessing>> GetListAsync(string lineId, int pageIndex = 1, int pageSize = 10)
        {
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            RefAsync<int> total = 0;
            return await _db.Queryable<Blessing>()
                .Where(x => x.LineId == lineId)
                .OrderBy(x => x.CreationTime, OrderByType.Desc)
                .ToPageListAsync(pageIndex, pageSize, total).ConfigureAwait(false);
        }
    }
}
