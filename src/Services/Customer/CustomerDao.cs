using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SqlSugar;
using Wedding.Data;

namespace Wedding.Services
{
    public class CustomerDao : SqlSugarBaseDao<Customer>, ICustomerDao
    {
        public CustomerDao() : base(null) { }

        public CustomerDao(IOptions<ConnectionConfig> config) : base(config) { }

        public Task<Customer> GetByLineIdAsync(string lineId)
        {
            return _db.Queryable<Customer>().FirstAsync(x => x.LineId == lineId);
        }

        public new Task<Customer> AddAsync(Customer customer)
        {
            customer.CreationTime = customer.LastModifyTime = System.DateTime.UtcNow;
            return base.AddAsync(customer);
        }

        public Task<Customer> UpdateAsync(Customer customer, string lineId)
        {
            customer.LastModifyTime = System.DateTime.UtcNow;
            _db.Updateable(customer).ExecuteCommandAsync();
            return GetByLineIdAsync(lineId);
        }

        public async Task<IList<Customer>> GetListAsync(int pageIndex = 1, int pageSize = 10)
        {
            RefAsync<int> total = 0;
            return await _db.Queryable<Customer>().ToPageListAsync(pageIndex, pageSize, total).ConfigureAwait(false);
        }
    }
}