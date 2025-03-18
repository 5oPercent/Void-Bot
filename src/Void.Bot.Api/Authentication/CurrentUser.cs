using System.Security.Claims;
using Void.Bot.Core.Services.Identity;

namespace Void.Bot.Api.Authentication;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? Id
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new ApplicationException("HttpContext is not available");
            }

            var idClaim = _httpContextAccessor.HttpContext.User.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier);

            return Guid.Parse(idClaim.Value);
        }
    }

    public string UserName
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                throw new ApplicationException("HttpContext is not available");
            }
            
            var userNameClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);

            return userNameClaim.Value;
        }
    }
}