namespace Void.Bot.Message.Dto.Bungie;

public class BungieApiNetUserRequest
{
    public required string Id { get; set; }
}

public class BungieApiNetUserResponse : BungieApiResponse
{
    public BungieNetUserDto? Response { get; set; }
}