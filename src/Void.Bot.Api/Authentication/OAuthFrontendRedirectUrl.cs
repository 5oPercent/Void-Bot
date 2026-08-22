using Microsoft.AspNetCore.WebUtilities;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Identity;

namespace Void.Bot.Api.Authentication;

public static class OAuthFrontendRedirectUrl
{
    public static string Create(
        OAuthSetting setting,
        AppPlatformEnum platform,
        OAuthCallbackResponse response)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["platform"] = platform.ToString().ToLowerInvariant()
        };

        if (response.Data is { } data)
        {
            parameters["flow"] = data.Flow.ToString().ToLowerInvariant();
            if (data.Flow == OAuthStateFlow.Register)
            {
                parameters["result"] = "registration_required";
                parameters["code"] = data.RegistrationCode;
            }
            else
            {
                parameters["result"] = "success";
                if (data.Flow == OAuthStateFlow.Login)
                {
                    parameters["code"] = data.ExchangeCode;
                }
            }
        }
        else
        {
            parameters["result"] = "failed";
            parameters["error"] = string.IsNullOrWhiteSpace(response.Message)
                ? "provider_failure"
                : response.Message;
        }

        return QueryHelpers.AddQueryString(GetCallbackUrl(setting), parameters);
    }

    public static string CreateFailure(
        OAuthSetting setting,
        string platform,
        string error)
    {
        return QueryHelpers.AddQueryString(
            GetCallbackUrl(setting),
            new Dictionary<string, string?>
            {
                ["platform"] = platform.ToLowerInvariant(),
                ["result"] = "failed",
                ["error"] = error
            });
    }

    private static string GetCallbackUrl(OAuthSetting setting)
    {
        if (!Uri.TryCreate(setting.FrontendCallbackUrl, UriKind.Absolute, out var callbackUri) ||
            (callbackUri.Scheme != Uri.UriSchemeHttps &&
             !(callbackUri.Scheme == Uri.UriSchemeHttp && callbackUri.IsLoopback)) ||
            !string.IsNullOrEmpty(callbackUri.UserInfo))
        {
            throw new InvalidOperationException("OAuth frontend callback URL is not configured correctly.");
        }

        return callbackUri.ToString();
    }
}
