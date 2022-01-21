using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SqlSugar;
using Wedding.Data;

namespace Wedding.Services
{
    public abstract class SqlSugarBaseDao<T> : IBaseDao<T> where T : class, new()
    {
        protected readonly SqlSugarClient _db;

        protected SqlSugarBaseDao(IOptions<ConnectionConfig> config)
        {
            var currentConfig = config?.Value;
            currentConfig ??= new ConnectionConfig
            {
                ConnectionString = "DataSource=.\\db.sqlite;",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            };
            _db = new SqlSugarClient(currentConfig);
            _db.CodeFirst.InitTables<Customer>();
            _db.CodeFirst.InitTables<Blessing>();
        }

        public Task<T> GetAsync(Expression<Func<T, bool>> where)
        {
            return _db.Queryable<T>().FirstAsync(where);
        }

        public Task<T> AddAsync(T obj)
        {
            return _db.Insertable(obj).ExecuteReturnEntityAsync();
        }

        public Task<int> RemoveAsync(T obj)
        {
            return _db.Deleteable(obj).ExecuteCommandAsync();
        }

        public Task<T> UpdateAsync(T obj, Expression<Func<T, bool>> where)
        {
            _db.Updateable(obj).ExecuteCommandAsync();
            return GetAsync(where);
        }
    }
}