using Void.Bot.Api.Filters;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.Social;
using Void.Bot.Message.Request.Social;

namespace Void.Bot.Api.Controllers;

[Authorize(AuthenticationSchemes = "Bearer")]
[SocialAccount]
[ApiController]
[Security]
[Route("api/social")]
[RequestSizeLimit(4096)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[EnableRateLimiting("social")]
public class SocialController : ControllerBase
{
    private readonly IMediator _mediator;

    public SocialController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("lookup")]
    [SkipPermissionCheck]
    [DescriptionLocalization("FindSocialAccount")]
    [ProducesResponseType(typeof(FindSocialAccountResponse), 200)]
    public async Task<IActionResult> FindSocialAccountAsync([FromQuery] FindSocialAccountRequest message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<FindSocialAccountRequest, FindSocialAccountResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("friends")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetFriends")]
    [ProducesResponseType(typeof(GetFriendsResponse), 200)]
    public async Task<IActionResult> GetFriendsAsync([FromQuery] GetFriendsRequest message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetFriendsRequest, GetFriendsResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("requests")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetFriendRequests")]
    [ProducesResponseType(typeof(GetFriendRequestsResponse), 200)]
    public async Task<IActionResult> GetFriendRequestsAsync([FromQuery] GetFriendRequestsRequest message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetFriendRequestsRequest, GetFriendRequestsResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("blocks")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetAccountBlocks")]
    [ProducesResponseType(typeof(GetAccountBlocksResponse), 200)]
    public async Task<IActionResult> GetAccountBlocksAsync([FromQuery] GetAccountBlocksRequest message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetAccountBlocksRequest, GetAccountBlocksResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("requests")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SendFriendRequest")]
    [ProducesResponseType(typeof(SendFriendRequestResponse), 200)]
    public async Task<IActionResult> SendFriendRequestAsync([FromBody] SendFriendRequestCommand message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SendFriendRequestCommand, SendFriendRequestResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("requests/{friendRequestId:guid}/accept")]
    [SkipPermissionCheck]
    [DescriptionLocalization("AcceptFriendRequest")]
    [ProducesResponseType(typeof(AcceptFriendRequestResponse), 200)]
    public async Task<IActionResult> AcceptFriendRequestAsync(Guid friendRequestId, [FromBody] AcceptFriendRequestCommand message,
        CancellationToken cancellationToken)
    {
        message.FriendRequestId = friendRequestId;
        var response = await _mediator.SendAsync<AcceptFriendRequestCommand, AcceptFriendRequestResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("requests/{friendRequestId:guid}/reject")]
    [SkipPermissionCheck]
    [DescriptionLocalization("RejectFriendRequest")]
    [ProducesResponseType(typeof(RejectFriendRequestResponse), 200)]
    public async Task<IActionResult> RejectFriendRequestAsync(Guid friendRequestId, [FromBody] RejectFriendRequestCommand message,
        CancellationToken cancellationToken)
    {
        message.FriendRequestId = friendRequestId;
        var response = await _mediator.SendAsync<RejectFriendRequestCommand, RejectFriendRequestResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("requests/{friendRequestId:guid}/withdraw")]
    [SkipPermissionCheck]
    [DescriptionLocalization("WithdrawFriendRequest")]
    [ProducesResponseType(typeof(WithdrawFriendRequestResponse), 200)]
    public async Task<IActionResult> WithdrawFriendRequestAsync(Guid friendRequestId, [FromBody] WithdrawFriendRequestCommand message,
        CancellationToken cancellationToken)
    {
        message.FriendRequestId = friendRequestId;
        var response = await _mediator.SendAsync<WithdrawFriendRequestCommand, WithdrawFriendRequestResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("friends/{targetAccountId:guid}/remove")]
    [SkipPermissionCheck]
    [DescriptionLocalization("RemoveFriend")]
    [ProducesResponseType(typeof(RemoveFriendResponse), 200)]
    public async Task<IActionResult> RemoveFriendAsync(Guid targetAccountId, [FromBody] RemoveFriendCommand message,
        CancellationToken cancellationToken)
    {
        message.TargetAccountId = targetAccountId;
        var response = await _mediator.SendAsync<RemoveFriendCommand, RemoveFriendResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("blocks")]
    [SkipPermissionCheck]
    [DescriptionLocalization("BlockAccount")]
    [ProducesResponseType(typeof(BlockAccountResponse), 200)]
    public async Task<IActionResult> BlockAccountAsync([FromBody] BlockAccountCommand message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<BlockAccountCommand, BlockAccountResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("blocks/{targetAccountId:guid}/remove")]
    [SkipPermissionCheck]
    [DescriptionLocalization("UnblockAccount")]
    [ProducesResponseType(typeof(UnblockAccountResponse), 200)]
    public async Task<IActionResult> UnblockAccountAsync(Guid targetAccountId, [FromBody] UnblockAccountCommand message,
        CancellationToken cancellationToken)
    {
        message.TargetAccountId = targetAccountId;
        var response = await _mediator.SendAsync<UnblockAccountCommand, UnblockAccountResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

}
