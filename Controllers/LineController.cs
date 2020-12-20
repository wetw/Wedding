using Microsoft.AspNetCore.Mvc;
using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Filters;
using NetCoreLineBotSDK.Models;
using System.Threading.Tasks;

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

        [HttpPost]
        [LineVerifySignature]
        public async Task<IActionResult> Post(WebhookEvent request)
        {
            await _app.RunAsync(request.events).ConfigureAwait(false);
            return Ok();
        }
    }
}