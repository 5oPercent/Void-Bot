using Void.Bot.Message.Commands.Social;
using Void.Bot.Message.Request.Social;

namespace Void.Bot.Core.Services.Social;

public interface IFriendService : IScope
{
    Task<FindSocialAccountResponse> FindSocialAccountAsync(FindSocialAccountRequest message, CancellationToken cancellationToken);
    Task<GetFriendsResponse> GetFriendsAsync(GetFriendsRequest message, CancellationToken cancellationToken);
    Task<GetFriendRequestsResponse> GetFriendRequestsAsync(GetFriendRequestsRequest message, CancellationToken cancellationToken);
    Task<GetAccountBlocksResponse> GetAccountBlocksAsync(GetAccountBlocksRequest message, CancellationToken cancellationToken);
    Task<SendFriendRequestResponse> SendFriendRequestAsync(SendFriendRequestCommand message, CancellationToken cancellationToken);
    Task<AcceptFriendRequestResponse> AcceptFriendRequestAsync(AcceptFriendRequestCommand message, CancellationToken cancellationToken);
    Task<RejectFriendRequestResponse> RejectFriendRequestAsync(RejectFriendRequestCommand message, CancellationToken cancellationToken);
    Task<WithdrawFriendRequestResponse> WithdrawFriendRequestAsync(WithdrawFriendRequestCommand message, CancellationToken cancellationToken);
    Task<RemoveFriendResponse> RemoveFriendAsync(RemoveFriendCommand message, CancellationToken cancellationToken);
    Task<BlockAccountResponse> BlockAccountAsync(BlockAccountCommand message, CancellationToken cancellationToken);
    Task<UnblockAccountResponse> UnblockAccountAsync(UnblockAccountCommand message, CancellationToken cancellationToken);
}
