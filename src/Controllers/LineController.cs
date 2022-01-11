using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Filters;
using NetCoreLineBotSDK.Models;
using Wedding.Data;
using Wedding.Services;

namespace Wedding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LineController : ControllerBase
    {
        private readonly ICustomerDao _customerDao;
        private readonly ILogger<LineController> _logger;
        private readonly LineBotApp _app;

        public LineController(LineBotApp app, ICustomerDao customerDao, ILogger<LineController> logger)
        {
            _app = app;
            _customerDao = customerDao;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        [LineVerifySignature]
        public async Task<IActionResult> Post(WebhookEvent request)
        {
            await _app.RunAsync(request.events).ConfigureAwait(false);
            return Ok();
        }

        [HttpGet("login")]
        public async Task<IActionResult> LoginAsync(string returnUrl = null)
        {
            try
            {
                var customer = User.ToCustomer();
                _logger.LogInformation($"{customer.Name} Login success.");
                if (!string.IsNullOrWhiteSpace(customer?.LineId)
                    && await _customerDao.GetByLineIdAsync(customer.LineId).ConfigureAwait(false) is null)
                {
                    await _customerDao.AddAsync(customer).ConfigureAwait(false);
                }
            }
            catch (DbException e)
            {
                _logger.LogError(e, "Add customer error when Login.");
            }
            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }

        [AllowAnonymous]
        [HttpGet("logout")]
        public async Task<IActionResult> LogoutAsync(string returnUrl = null)
        {
            await HttpContext.SignOutAsync().ConfigureAwait(false);
            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }
    }
}