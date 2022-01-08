using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Filters;
using NetCoreLineBotSDK.Models;
using System.Security.Claims;
using System.Threading.Tasks;
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
        private readonly LineBotApp _app;

        public LineController(LineBotApp app, ICustomerDao customerDao)
        {
            _app = app;
            _customerDao = customerDao;
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
                if (await _customerDao.GetByLineIdAsync(customer.LineId).ConfigureAwait(false) == null)
                {
                    await _customerDao.AddAsync(customer).ConfigureAwait(false);
                }
            }
            catch
            {
            }
            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");

        }
    }
}