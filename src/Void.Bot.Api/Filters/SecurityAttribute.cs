using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Dto;

namespace Void.Bot.Api.Filters;

public class SecurityAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUser>();
        if (currentUser?.Id == null || currentUser.Id == Guid.Empty)
        {
            return;
        }
        
        var path = context.HttpContext.Request.Path.Value?.ToLower() ?? string.Empty;
        var method = context.HttpContext.Request.Method.ToUpper();
        
        var securityService = context.HttpContext.RequestServices.GetService<ISecurityService>()!;
        
        var isAllow = await securityService.VerifyPermissionEndpointAsync(
            currentUser.Id.Value, path, method, context.HttpContext.RequestAborted).ConfigureAwait(false);
        
        if (!isAllow)
        {
            var response = new VoidBotResponse
            {
                Code = HttpStatusCode.Forbidden,
                Message = "You do not have permission to access."
            };
            
            context.Result = new ObjectResult(response);
        }
    }
}