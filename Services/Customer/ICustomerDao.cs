using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Wedding.Data;

namespace Wedding.Services.Customer
{
    public interface ICustomerDao
    {
        Task<Data.Customer> GetByLineIdAsync(string lineId);

        Task<Data.Customer> AddAsync(Data.Customer customer);

        Task<Data.Customer> UpdateAsync(Data.Customer customer, string lineId);

        Task<int> RemoveAsync(Data.Customer customer);
    }
}