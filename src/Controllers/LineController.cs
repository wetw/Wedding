using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Filters;
using NetCoreLineBotSDK.Models;
using System.Threading.Tasks;

namespace Wedding.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LineController : ControllerBase
    {
        private readonly LineBotApp _app;

        public LineController(LineBotApp app)
        {
            _app = app;
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
        public IActionResult Login(string returnUrl = null)
        {
            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }
    }
}