using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetCoreLineBotSDK;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Wedding.Data;
using Wedding.Services;
using Wedding.Services.LineBot;

namespace Wedding
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<WeddingOptions>(Configuration.GetSection(nameof(WeddingOptions)));
            services.Configure<LineBotSetting>(Configuration.GetSection(nameof(LineBotSetting)));
            services.AddRazorPages();
            services.AddControllers();
            services.AddServerSideBlazor();
            services.AddLineBotSDK(Configuration);
            services.AddSingleton<LineBotApp, WeddingLineBotApp>();
            services.AddSingleton<CountDownService>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Line";
            })
            .AddCookie()
            .AddOAuth("Line", options =>
                {
                    options.ClientId = Configuration.GetValue("LineBotSetting:ClientId", "");
                    options.ClientSecret = Configuration.GetValue("LineBotSetting:ClientSecret", "");
                    options.CallbackPath = new PathString(Configuration.GetValue("LineBotSetting:CallbackPath", "/api/line/auth"));
                    options.AuthorizationEndpoint = Configuration.GetValue("LineBotSetting:AuthorizationEndpoint", "https://access.line.me/oauth2/v2.1/authorize");
                    options.TokenEndpoint = Configuration.GetValue("LineBotSetting:TokenEndpoint", "https://api.line.me/oauth2/v2.1/token");
                    options.UserInformationEndpoint = Configuration.GetValue("LineBotSetting:UserInformationEndpoint", "https://api.line.me/v2/profile");
                    options.SaveTokens = true;
                    foreach (var scope in Configuration.GetSection("LineBotSetting:Scopes").GetChildren().Select(c => c.Value))
                    {
                        options.Scope.Add(scope);
                    }
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "userId");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "displayName");
                    options.ClaimActions.MapJsonKey("PictureUrl", "pictureUrl");
                    options.ClaimActions.MapJsonKey("StatusMessage", "statusMessage");
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted).ConfigureAwait(false);
                            response.EnsureSuccessStatusCode();
                            var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                            context.RunClaimActions(user.RootElement);
                            // email 資訊不在 profile 內, https://developers.line.biz/zh-hant/docs/line-login/integrate-line-login/#verify-id-token
                            //if (options.Scope.Contains("email"))
                            //{
                            //    var tokens = context.Properties.GetTokens().ToList();
                            //    var jwtSecurityToken = new JwtSecurityToken(context.AccessToken);
                            //    context.Identity.AddClaim(new Claim(ClaimTypes.Email, jwtSecurityToken.Claims.First(c => c.Type == "email")?.Value ?? string.Empty));
                            //}
                        },
                        OnRemoteFailure = context =>
                        {
                            var failure = context.Failure;
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<OAuthEvents>>();
                            logger.LogError(failure, failure?.Message);
                            context.Response.Redirect("/");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                });
            if (!string.IsNullOrWhiteSpace(
                Configuration.GetValue("ApplicationInsights:InstrumentationKey", string.Empty)))
            {
                services.AddApplicationInsightsTelemetry();
            }

            if (Configuration.GetValue("Azure:SignalR:Enabled", false)
                && !string.IsNullOrWhiteSpace(Configuration.GetValue("Azure:SignalR:ConnectionString", string.Empty)))
            {
                services.AddSignalR().AddAzureSignalR();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                // This is needed if running behind a reverse proxy
                // like ngrok which is great for testing while developing
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    RequireHeaderSymmetry = false,
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }
            else
            {
                // 避免 oauth 的 redirect_uri 抓到的是 http
                app.Use(next => context =>
                {
                    if (string.Equals(context.Request.Headers["X-Forwarded-Proto"], "https", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Request.Scheme = "https";
                    }
                    return next(context);
                });
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                //加上MapDefaultControllerRoute()
                endpoints.MapDefaultControllerRoute();
                //支援透過Attribute指定路由
                endpoints.MapControllers();
            });
        }
    }
}
