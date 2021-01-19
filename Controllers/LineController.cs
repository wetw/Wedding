using isRock.LineLoginV21;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using NetCoreLineBotSDK;
using NetCoreLineBotSDK.Filters;
using NetCoreLineBotSDK.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Wedding.Data;
using Wedding.Pages;

namespace Wedding.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineController : ControllerBase
    {
        private readonly LineBotSetting _lineBotSetting;
        private readonly LineBotApp _app;

        public LineController(LineBotApp app, IOptionsMonitor<LineBotSetting> options)
        {
            _app = app;
            _lineBotSetting = options.CurrentValue;
        }

        [HttpPost("webhook")]
        [LineVerifySignature]
        public async Task<IActionResult> Post(WebhookEvent request)
        {
            await _app.RunAsync(request.events).ConfigureAwait(false);
            return Ok();
        }

        [HttpGet("auth")]
        public IActionResult Auth()
        {
            var query = QueryHelpers.ParseQuery(Request.QueryString.Value ?? string.Empty);
            if (!query.TryGetValue("code", out var code))
            {
                return Ok("登入失敗");
            }

            var token = Utility.GetTokenFromCode(code,
                _lineBotSetting.ClientId,
                _lineBotSetting.ClientSecret,
                Url.ActionLink(nameof(Auth), protocol: "https").ToLowerInvariant());

            var user = (ExtendProfile)Utility.GetUserProfile(token.access_token);
            var jwtSecurityToken = new JwtSecurityToken(token.id_token);
            user.Email = jwtSecurityToken.Claims.First(c => c.Type == "email")?.Value ?? string.Empty;
            return Redirect($"~/{nameof(Survey)}");
        }
    }
    public partial class ExtendProfile : UserProfile
    {
        public string Email { get; set; }

        public static explicit operator ExtendProfile(Profile profile)
        {
            return new()
            {
                displayName = profile.displayName,
                userId = profile.userId,
                statusMessage = profile.statusMessage,
                pictureUrl = profile.pictureUrl
            };
        }
    }
}