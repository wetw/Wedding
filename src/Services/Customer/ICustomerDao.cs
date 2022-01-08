using System.Threading.Tasks;
using Wedding.Data;

namespace Wedding.Services
{
    public interface ICustomerDao
    {
        Task<Customer> GetByLineIdAsync(string lineId);

        Task<Customer> AddAsync(Customer customer);

        Task<Customer> UpdateAsync(Customer customer, string lineId);

        Task<int> RemoveAsync(Customer customer);
    }
}