using System.Threading.Tasks;
using GridBlazor;
using GridShared;
using GridShared.Utility;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Primitives;
using Wedding.Data;
using Wedding.Services;

namespace Wedding.Pages
{
    partial class ManagePage
    {
        [Inject]
        private ICustomerDao CustomerDao { get; init; }

        [Inject]
        private NavigationManager NavigationManager { get; init; }

        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; init; }

        private CGrid<Customer> _grid;

        private static void Columns(IGridColumnCollection<Customer> c)
        {
            c.Add(x => x.Id);
            c.Add(x => x.IsAttend).Titled("參加");
            c.Add(x => x.Name).Titled("Line 名稱");
            c.Add(x => x.RealName).Titled("名稱");
            c.Add(x => x.Relation).Titled("關係");
            c.Add(x => x.IsEmailBook).Titled("電子喜帖").Sum(true);
            c.Add(x => x.Email).Titled("信箱");
            c.Add(x => x.IsRealBook).Titled("實體喜帖").Sum(true);
            c.Add(x => x.PostCode).Titled("郵遞區號");
            c.Add(x => x.Address).Titled("地址");
            c.Add(x => x.Visitors).Titled("大人人數").Sum(true);
            c.Add(x => x.ChildrenCount).Titled("小孩人數").Sum(true);
            c.Add(x => x.VegetarianCount).Titled("吃素人數").Sum(true);
            c.Add(x => x.LastModifyTime).Titled("最後更新時間").SetFilterWidgetType("DateTimeLocal");
        }

        protected override async Task OnInitializedAsync()
        {
            var authenticationState = await AuthenticationStateTask.ConfigureAwait(false);
            if (authenticationState?.User?.Identity is null
                || !authenticationState.User.Identity.IsAuthenticated)
            {
                var returnUrl = $"/{NavigationManager.ToBaseRelativePath(NavigationManager.Uri)}";
                if (string.IsNullOrWhiteSpace(returnUrl))
                {
                    NavigationManager.NavigateTo("api/line/login", true);
                }
                NavigationManager.NavigateTo($"/api/line/login?returnUrl={returnUrl}", true);
            }
        }

        protected override Task OnParametersSetAsync()
        {
            var query = new QueryDictionary<StringValues>();
            var client = new GridClient<Customer>(q => CustomerDao.GetGridRowsAsync(Columns, q), query, false, "customersGrid", Columns)
                .SetRowCssClasses(customer => customer.IsAttend == null ? "IsAttendNull" : string.Empty)
                .ChangePageSize(true)
                .Sortable()
                .Filterable()
                .WithMultipleFilters()
                .Groupable(true)
                .ClearFiltersButton(true)
                .SetStriped(true)
                .WithGridItemsCount()
                .SetExcelExport(true, false, "Customers")
                .Searchable(true, false, true);
            _grid = client.Grid;

            // Set new items to grid
            _ = client.UpdateGrid();
            return Task.CompletedTask;
        }
    }

}
