using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Services.Identity;

public class HttpCurrentUser : ICurrentUser
{
    private const string LanguageHeader = "Accept-Language";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? Id
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return null;
            }

            var idClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);

            return Guid.TryParse(idClaim?.Value, out var id) ? id : null;
        }
    }

    public string UserName
    {
        get
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return string.Empty;
            }

            var userNameClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);

            return userNameClaim?.Value ?? string.Empty;
        }
    }

    public bool IsTwoFactorAuthenticated => _httpContextAccessor.HttpContext?.User.HasClaim(
        TokenClaimService.TwoFactorAuthenticationClaim,
        TokenClaimService.TwoFactorAuthenticationValue) == true;

    public LanguageEnum Language
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.Request.Headers[LanguageHeader].ToString();
            var language = value?.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
            return LanguageExtension.MapI18nCodeToEnum(language);
        }
    }
}
