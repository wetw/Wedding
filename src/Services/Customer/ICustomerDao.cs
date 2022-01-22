using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GridShared;
using GridShared.Utility;
using Microsoft.Extensions.Primitives;
using Wedding.Data;

namespace Wedding.Services
{
    public interface ICustomerDao
    {
        Task<Customer> GetByLineIdAsync(string lineId);

        Task<Customer> AddAsync(Customer customer);

        Task<Customer> UpdateAsync(Customer customer, string lineId);

        Task<int> RemoveAsync(Customer customer);

        Task<IList<Customer>> GetListAsync(int pageIndex = 1, int pageSize = 10);

        ItemsDTO<Customer> GetGridRowsAsync(
            Action<IGridColumnCollection<Customer>> columns,
            QueryDictionary<StringValues> query);
    }
}