using System.Security.Claims;
using System.Threading.RateLimiting;
using Void.Bot.Api.HostedService;
using Void.Bot.Api.Hubs;
using Void.Bot.Core.Services.Social;

namespace Void.Bot.Api.Extension;

public static class SocialNotificationExtensions
{
    public static IServiceCollection AddSocialNotifications(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = false;
            options.MaximumReceiveMessageSize = 4096;
            options.MaximumParallelInvocationsPerClient = 1;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(45);
        });
        services.AddSingleton<SocialConnectionRegistry>();
        services.AddSingleton<ISocialNotificationTransport, SignalRSocialNotificationTransport>();
        services.AddHostedService<SocialConnectionHostedService>();
        services.AddHostedService<SocialNotificationHostedService>();
        services.AddHostedService<EraSocialNotificationHostedService>();
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("social", context =>
            {
                var account = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var partition = account ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
                return RateLimitPartition.GetFixedWindowLimiter(partition, key => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 30,
                    Window = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
            });
        });
        // 浏览器跨域时显式配置受信任来源；未配置时只支持同源 Web。
        var origins = configuration.GetSection("Social:AllowedWebOrigins").Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options =>
        {
            options.AddPolicy("social-web", policy =>
            {
                if (origins.Length > 0)
                {
                    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                }
            });
        });
        return services;
    }

    public static WebApplication MapSocialNotifications(this WebApplication app)
    {
        app.MapHub<WebSocialHub>("/hubs/web/social", options =>
        {
            options.CloseOnAuthenticationExpiration = true;
            options.ApplicationMaxBufferSize = 16384;
            options.TransportMaxBufferSize = 16384;
        }).RequireCors("social-web").RequireRateLimiting("social");
        app.MapHub<EraSocialHub>("/hubs/era/social", options =>
        {
            options.CloseOnAuthenticationExpiration = true;
            options.ApplicationMaxBufferSize = 16384;
            options.TransportMaxBufferSize = 16384;
        }).RequireCors("social-web").RequireRateLimiting("social");
        return app;
    }
}
