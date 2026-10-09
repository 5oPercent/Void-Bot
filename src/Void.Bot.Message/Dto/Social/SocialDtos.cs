using Void.Bot.Message.Enum.Social;

namespace Void.Bot.Message.Dto.Social;

public class SocialAccountDto
{
    public Guid AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Suffix { get; set; } = string.Empty;
}

public class FriendDto
{
    public SocialAccountDto Account { get; set; } = new SocialAccountDto();
    public DateTimeOffset FriendsSince { get; set; }
}

public class FriendRequestDto
{
    public Guid Id { get; set; }
    public SocialAccountDto Sender { get; set; } = new SocialAccountDto();
    public SocialAccountDto Recipient { get; set; } = new SocialAccountDto();
    public FriendRequestStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public long Version { get; set; }
}

public class SocialPageDto<T>
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public List<T> Items { get; set; } = new List<T>();
}
