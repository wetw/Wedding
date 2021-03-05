using SqlSugar;
using System.Threading.Tasks;

namespace Wedding.Services.Customer
{
    public class CustomerDao : SqlSugarBaseDao<Data.Customer>, ICustomerDao
    {
        public CustomerDao() : base(null) { }

        public CustomerDao(ConnectionConfig config) : base(config) { }

        public Task<Data.Customer> GetByLineIdAsync(string lineId)
        {
            return _db.Queryable<Data.Customer>().FirstAsync(x => x.LineId == lineId);
        }

        public Task<Data.Customer> UpdateAsync(Data.Customer customer, string lineId)
        {
            _db.Updateable(customer).ExecuteCommandAsync();
            return GetByLineIdAsync(lineId);
        }
    }
}