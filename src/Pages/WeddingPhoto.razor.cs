using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Wedding.Data;

namespace Wedding.Pages
{
    public partial class WeddingPhoto
    {
        [Inject]
        private IOptionsMonitor<WeddingOptions> Options { get; init; }

        private IReadOnlyCollection<object> _dataSource;

        protected override void OnInitialized()
        {
            _dataSource = Options.CurrentValue
                .WeddingPhotos.Select(x => (object)new { image = x }).ToList();
        }
    }
}