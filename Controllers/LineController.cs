using Microsoft.AspNetCore.Mvc;
using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Filters;
using NetCoreLineBotSDK.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Wedding.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineController : ControllerBase
    {
        private readonly LineBotApp _app;

        public LineController(LineBotApp app)
        {
            _app = app;
        }

        [HttpPost("webhook")]
        [LineVerifySignature]
        public async Task<IActionResult> Post(WebhookEvent request)
        {
            await _app.RunAsync(request.events).ConfigureAwait(false);
            return Ok();
        }

        [Authorize]
        [HttpGet("login")]
        public IActionResult Login(string returnUrl = null)
        {
            return Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : $"~/{returnUrl}");
        }
    }
}