using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SqlSugar;
using Wedding.Data;

namespace Wedding.Services
{
    public abstract class SqlSugarBaseDao<T> : IBaseDao<T> where T : class, new()
    {
        protected readonly SqlSugarClient _db;

        protected SqlSugarBaseDao(ConnectionConfig config)
        {
            config ??= new ConnectionConfig
            {
                ConnectionString = "DataSource=.\\db.sqlite;",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            };
            _db = new SqlSugarClient(config);
            _db.CodeFirst.InitTables<Data.Customer>();
            _db.CodeFirst.InitTables<Blessing>();
        }

        public Task<T> GetAsync(Expression<Func<T, bool>> where)
        {
            return _db.Queryable<T>().FirstAsync(where);
        }

        public Task<T> AddAsync(T obj)
        {
            return _db.Insertable<T>(obj).ExecuteReturnEntityAsync();
        }

        public Task<int> RemoveAsync(T obj)
        {
            return _db.Deleteable<T>(obj).ExecuteCommandAsync();
        }

        public Task<T> UpdateAsync(T obj, Expression<Func<T, bool>> where)
        {
            _db.Updateable(obj).ExecuteCommandAsync();
            return GetAsync(where);
        }
    }
}