using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Smart.Blazor;
using Wedding.Data;
using Wedding.Services;

namespace Wedding.Pages
{
    partial class ManagePage
    {

        [Inject]
        private ICustomerDao CustomerDao { get; init; }

        private readonly TableColumn[] _columns = new TableColumn[]{
            new () {
                Label = "Id",
                DataField = nameof(Customer.Id),
            },
            new () {
                Label = "Name",
                DataField = nameof(Customer.RealName),
            }
        };

        private IList<Customer> _customers;

        protected override async Task OnInitializedAsync()
        {
            _customers = await CustomerDao.GetListAsync(1, 10).ConfigureAwait(false);
        }
    }

}
