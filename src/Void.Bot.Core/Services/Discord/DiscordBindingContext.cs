using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Core.Services.Discord;

public interface IDiscordBindingContext : IScope
{
    UserThirdPartyDto Binding { get; set; }
}

public sealed class DiscordBindingContext : IDiscordBindingContext
{
    public UserThirdPartyDto Binding { get; set; } = null!;
}
