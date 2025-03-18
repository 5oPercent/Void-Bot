using Microsoft.AspNetCore.Authentication;
using Void.Bot.Api.Authentication;

namespace Void.Bot.Api.Extension;

public static class ApiKeyExtensions
{
    public static AuthenticationBuilder AddApiKeyScheme(this AuthenticationBuilder builder)
    {
        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.AuthenticationScheme, _ => { });
    }
}