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
        protected static SqlSugarScope Db;

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
            Db = new SqlSugarScope(currentConfig);
            Db.CodeFirst.InitTables<Customer>();
            Db.CodeFirst.InitTables<Blessing>();
        }

        public Task<T> GetAsync(Expression<Func<T, bool>> where)
        {
            return Db.Queryable<T>().FirstAsync(where);
        }

        public Task<T> AddAsync(T obj)
        {
            return Db.Insertable(obj).ExecuteReturnEntityAsync();
        }

        public Task<int> RemoveAsync(T obj)
        {
            return Db.Deleteable(obj).ExecuteCommandAsync();
        }

        public Task<T> UpdateAsync(T obj, Expression<Func<T, bool>> where)
        {
            Db.Updateable(obj).ExecuteCommandAsync();
            return GetAsync(where);
        }
    }
}