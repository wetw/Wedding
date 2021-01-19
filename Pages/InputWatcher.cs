using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;

namespace Wedding.Pages
{
    public class InputWatcher : ComponentBase
    {
        private EditContext _editContext;

        [CascadingParameter]
        protected EditContext EditContext
        {
            get => _editContext;
            set
            {
                _editContext = value;
                EditContextActionChanged?.Invoke(_editContext);
            }
        }

        [Parameter]
        public Action<EditContext> EditContextActionChanged { get; set; }
    }
}
