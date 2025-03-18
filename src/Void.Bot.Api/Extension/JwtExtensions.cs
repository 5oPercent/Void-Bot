using Microsoft.AspNetCore.Authentication;
using Void.Bot.Api.Authentication;

namespace Void.Bot.Api.Extension;

public static class JwtExtensions
{
    public static AuthenticationBuilder AddJwtScheme(this AuthenticationBuilder builder)
    {
        builder.AddScheme<CustomizeJwtAuthenticationOptions, CustomizeJwtAuthenticationHandler>(
            CustomizeJwtAuthenticationOptions.AuthenticationScheme, _ => { });
        
        return builder;
    }
}