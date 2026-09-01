using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Void.Bot.Api.Authentication;
using Void.Bot.Core.Services.Identity.OAuth;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Dto;
using ILogger = Serilog.ILogger;

namespace Void.Bot.Api.ActionFilters;

public class UnifyResponseFilter : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult { Value: not VoidBotResponse } objectResult)
        {
            context.Result = new JsonResult(new VoidBotResponse<object>
            {
                Data = objectResult.Value,
                Code = HttpStatusCode.OK,
                Message = nameof(HttpStatusCode.OK).ToLower()
            });
        }
        
        base.OnActionExecuted(context);
    }
}

public class UnifyResponseExceptionFilter : IExceptionFilter
{
    private readonly ILogger _logger;
    private readonly IReadOnlyCollection<IOAuthProvider> _providers;
    private readonly OAuthSetting _oauthSetting;

    public UnifyResponseExceptionFilter(
        ILogger logger,
        IEnumerable<IOAuthProvider> providers,
        OAuthSetting oauthSetting)
    {
        _logger = logger;
        _providers = providers.ToArray();
        _oauthSetting = oauthSetting;
    }
    
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is BusinessException businessException)
        {
            var response = new VoidBotResponse
            {
                Code = businessException.Code,
                Message = businessException.Message
            };
            context.Result = new JsonResult(response);
            context.ExceptionHandled = true;
            return;
        }

        if (ThirdPartyAuthenticationBase.TryGetPlatform(context.HttpContext.Request.Path, out var platform))
        {
            _logger.Warning(
                "===== Third-party OAuth callback exception {Platform} {Action}: {ExceptionType}",
                platform,
                context.ActionDescriptor.DisplayName,
                context.Exception.GetType().Name);

            var provider = _providers.FirstOrDefault(item =>
                item.Platform.ToString().Equals(platform, StringComparison.OrdinalIgnoreCase));
            if (provider is not null)
            {
                if (context.HttpContext.Request.Path.Value?.EndsWith(
                        "/callback",
                        StringComparison.OrdinalIgnoreCase) == true)
                {
                    context.Result = new RedirectResult(
                        OAuthFrontendRedirectUrl.CreateFailure(
                            _oauthSetting,
                            platform,
                            "provider_failure"));
                    context.ExceptionHandled = true;
                    return;
                }

                var response = provider.CreateProviderFailure();
                var voidBotResponse = (VoidBotResponse)response;
                context.Result = new JsonResult(response)
                {
                    StatusCode = (int)voidBotResponse.Code
                };
                context.ExceptionHandled = true;
                return;
            }
        }

        _logger.Warning("===== Exception {Action}: {@Exception}", context.ActionDescriptor.DisplayName, context.Exception);
        context.Result = new JsonResult(new VoidBotResponse<object>
        {
            Data = context.Result,
            Code = HttpStatusCode.InternalServerError,
            Message = context.Exception.Message
        });
        context.ExceptionHandled = true;
    }
}
