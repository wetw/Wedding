using Microsoft.AspNetCore.Components.Forms;
using System;
using Wedding.Data;

namespace Wedding.Pages
{
    public partial class Survey
    {
        public EditContext LocalEditContext { get; set; }
        public string ValidationMessage { get; set; }
        private Customer Customer { get; set; }

        protected override void OnInitialized()
        {
            Customer = new Customer();
        }

        public void OnGet()
        {
            Console.WriteLine("123");
        }

        private void OnOK()
        {
            ValidationMessage = LocalEditContext.Validate()
                ? "表單驗證正確無誤"
                : "資料有錯，請重新修正";
        }

        private void OnEditContestChanged(EditContext context)
        {
            LocalEditContext = context;
        }
    }
}
