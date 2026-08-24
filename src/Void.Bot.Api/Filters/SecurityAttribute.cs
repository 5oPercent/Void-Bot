using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Dto;

namespace Void.Bot.Api.Filters;

public class SecurityAttribute() : TypeFilterAttribute(typeof(SecurityAuthorizationFilter));

public class SecurityAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly IUserRoleService _userRoleService;
    private readonly ISecurityService _securityService;

    public SecurityAuthorizationFilter(
        ICurrentUser currentUser, 
        IUserRoleService userRoleService, 
        ISecurityService securityService)
    {
        _currentUser = currentUser;
        _userRoleService = userRoleService;
        _securityService = securityService;
    }
    
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any() ||
            context.ActionDescriptor.EndpointMetadata.OfType<SkipPermissionCheckAttribute>().Any())
        {
            return;
        }

        if (_currentUser.Id == null || _currentUser.Id == Guid.Empty)
        {
            return;
        }

        if (await _userRoleService.CheckUserIsSuperAdminAsync(_currentUser.Id.GetValueOrDefault(), CancellationToken.None))
        {
            return;
        }
        
        var path = context.HttpContext.Request.Path.Value?.ToLower() ?? string.Empty;
        var method = context.HttpContext.Request.Method.ToUpper();

        var isAllow = await _securityService.VerifyPermissionEndpointAsync(
            _currentUser.Id.Value, path, method, context.HttpContext.RequestAborted);

        if (!isAllow)
        {
            var response = new VoidBotResponse
            {
                Code = HttpStatusCode.Forbidden,
                Message = "You do not have permission to access."
            };
            
            context.Result = new ObjectResult(response)
            {
                StatusCode = (int)HttpStatusCode.Forbidden
            };
        }
    }
}
