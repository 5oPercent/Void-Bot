using Void.Bot.Message.Dto;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Resources.i18n.Social;
using Void.Bot.Core.Services.Language;

namespace Void.Bot.Api.Filters;

public sealed class SocialAccountAttribute : TypeFilterAttribute
{
    public SocialAccountAttribute() : base(typeof(SocialAccountFilter))
    {
    }
}

public sealed class SocialAccountFilter : IAsyncActionFilter
{
    private readonly VoidBotDbContext _database;
    private readonly ILocalizationService _localization;

    public SocialAccountFilter(VoidBotDbContext database, ILocalizationService localization)
    {
        _database = database;
        _localization = localization;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var principal = context.HttpContext.User;
        var valid = Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) &&
            int.TryParse(principal.FindFirstValue("token_version"), out _);
        if (valid)
        {
            var version = int.Parse(principal.FindFirstValue("token_version")!);
            valid = await _database.Set<UserAccount>().AsNoTracking()
                .AnyAsync(value => value.Id == id && value.TokenVersion == version,
                    context.HttpContext.RequestAborted).ConfigureAwait(false);
        }
        if (!valid)
        {
            context.Result = new OkObjectResult(new VoidBotResponse<bool>
            {
                Code = HttpStatusCode.Unauthorized,
                ErrorCode = "social.unauthorized",
                Message = _localization.GetValue<SocialContextTemplate>(text => text.Unauthorized)
            });
            return;
        }
        await next().ConfigureAwait(false);
    }
}
