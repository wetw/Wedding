using SqlSugar;
using System.Threading.Tasks;
using Wedding.Data;

namespace Wedding.Services
{
    public class CustomerDao : SqlSugarBaseDao<Customer>, ICustomerDao
    {
        public CustomerDao() : base(null) { }

        public CustomerDao(ConnectionConfig config) : base(config) { }

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
    }
}