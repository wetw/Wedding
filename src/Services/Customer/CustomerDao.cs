using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GridCore.Server;
using GridShared;
using GridShared.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
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
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            return pageSize < 1
                ? await _db.Queryable<Customer>().ToListAsync().ConfigureAwait(false)
                : await _db.Queryable<Customer>().ToPageListAsync(pageIndex, pageSize, total).ConfigureAwait(false);
        }

        public ItemsDTO<Customer> GetGridRowsAsync(
            Action<IGridColumnCollection<Customer>> columns,
            QueryDictionary<StringValues> query)
        {
            var list = GetListAsync(1, 0).ConfigureAwait(false).GetAwaiter().GetResult();
            var server = new GridCoreServer<Customer>(list, new QueryCollection(query),
                true, "customersGrid", columns)
                .Sortable()
                .WithPaging(10)
                .Filterable()
                .Groupable(true)
                .WithGridItemsCount()
                .Searchable(true, true)
                .WithMultipleFilters();

            // return items to displays
            return server.ItemsToDisplay;
        }
    }
}