using Void.Bot.Message.Dto;
using System.Linq.Expressions;
using System.Net;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Domain.Social;
using Void.Bot.Core.Resources.i18n.Social;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.Social;
using Void.Bot.Message.Dto.Social;
using Void.Bot.Message.Enum.Social;
using Void.Bot.Message.Request.Social;

namespace Void.Bot.Core.Services.Social;

public class FriendService : IFriendService
{
    private const int FriendLimit = 1000;
    private const int PendingRequestLimit = 100;
    private readonly IFriendDataProvider _data;
    private readonly ISocialNotificationService _notifications;
    private readonly ICurrentUser _currentUser;
    private readonly ILocalizationService _localization;

    public FriendService(IFriendDataProvider data, ISocialNotificationService notifications,
        ICurrentUser currentUser, ILocalizationService localization)
    {
        _data = data;
        _notifications = notifications;
        _currentUser = currentUser;
        _localization = localization;
    }

    private T Error<T>(HttpStatusCode status, string code,
        Expression<Func<SocialContextTemplate, object>> text)
        where T : Void.Bot.Message.Dto.VoidBotResponse, new()
    {
        return new T
        {
            Code = status,
            ErrorCode = code,
            Message = _localization.GetValue(text)
        };
    }

    private Guid AccountId
    {
        get
        {
            return _currentUser.Id ?? Guid.Empty;
        }
    }

    private static SocialAccountDto MapAccount(UserAccount account)
    {
        return new SocialAccountDto
        {
            AccountId = account.Id,
            Name = account.Name,
            Suffix = account.Suffix
        };
    }

    private async Task<FriendRequestDto> MapRequestAsync(FriendRequest request,
        CancellationToken cancellationToken)
    {
        var sender = await _data.GetAccountAsync(request.SenderAccountId, cancellationToken).ConfigureAwait(false);
        var recipient = await _data.GetAccountAsync(request.RecipientAccountId, cancellationToken).ConfigureAwait(false);
        return new FriendRequestDto
        {
            Id = request.Id,
            Sender = sender == null ? new SocialAccountDto
            {
                AccountId = request.SenderAccountId
            } : MapAccount(sender),
            Recipient = recipient == null ? new SocialAccountDto
            {
                AccountId = request.RecipientAccountId
            } : MapAccount(recipient),
            Status = request.Status,
            CreatedAt = request.CreatedAt,
            ProcessedAt = request.ProcessedAt,
            Version = request.Version
        };
    }

    private async Task NotifyRequestAsync(FriendRequest request, CancellationToken cancellationToken)
    {
        await _notifications.EnqueueAsync(new[] { request.SenderAccountId, request.RecipientAccountId },
            "social.friend_request_changed", request.Id, request.Version, request.RequestId,
            cancellationToken).ConfigureAwait(false);
    }

    public async Task<FindSocialAccountResponse> FindSocialAccountAsync(FindSocialAccountRequest message,
        CancellationToken cancellationToken)
    {
        var separator = message.DisplayName.LastIndexOf('#');
        var account = await _data.FindAccountAsync(message.DisplayName[..separator],
            message.DisplayName[(separator + 1)..], cancellationToken).ConfigureAwait(false);
        if (account == null || await _data.IsBlockedAsync(AccountId, account.Id, cancellationToken).ConfigureAwait(false))
        {
            return Error<FindSocialAccountResponse>(HttpStatusCode.NotFound, "social.account_not_found",
                text => text.AccountNotFound);
        }
        return new FindSocialAccountResponse
        {
            Data = MapAccount(account)
        };
    }

    public async Task<GetFriendsResponse> GetFriendsAsync(GetFriendsRequest message, CancellationToken cancellationToken)
    {
        var rows = await _data.GetFriendshipsAsync(AccountId, message.PageIndex, message.PageSize, cancellationToken).ConfigureAwait(false);
        var page = new SocialPageDto<FriendDto>
        {
            PageIndex = message.PageIndex,
            PageSize = message.PageSize,
            TotalCount = rows.TotalCount
        };
        var accounts = await _data.GetAccountsAsync(rows.Items.Select(value =>
            value.FirstAccountId == AccountId ? value.SecondAccountId : value.FirstAccountId).ToArray(), cancellationToken).ConfigureAwait(false);
        foreach (var row in rows.Items)
        {
            var id = row.FirstAccountId == AccountId ? row.SecondAccountId : row.FirstAccountId;
            accounts.TryGetValue(id, out var account);
            if (account != null)
            {
                page.Items.Add(new FriendDto
                {
                    Account = MapAccount(account),
                    FriendsSince = row.CreatedAt
                });
            }
        }
        return new GetFriendsResponse
        {
            Data = page
        };
    }

    public async Task<GetFriendRequestsResponse> GetFriendRequestsAsync(GetFriendRequestsRequest message,
        CancellationToken cancellationToken)
    {
        var rows = await _data.GetRequestsAsync(AccountId, message.Outgoing, message.PageIndex, message.PageSize, cancellationToken).ConfigureAwait(false);
        var page = new SocialPageDto<FriendRequestDto>
        {
            PageIndex = message.PageIndex,
            PageSize = message.PageSize,
            TotalCount = rows.TotalCount
        };
        var accounts = await _data.GetAccountsAsync(rows.Items.SelectMany(value =>
            new[] { value.SenderAccountId, value.RecipientAccountId }).Distinct().ToArray(), cancellationToken).ConfigureAwait(false);
        foreach (var row in rows.Items)
        {
            accounts.TryGetValue(row.SenderAccountId, out var sender);
            accounts.TryGetValue(row.RecipientAccountId, out var recipient);
            page.Items.Add(new FriendRequestDto
            {
                Id = row.Id,
                Sender = sender == null ? new SocialAccountDto
                {
                    AccountId = row.SenderAccountId
                } : MapAccount(sender),
                Recipient = recipient == null ? new SocialAccountDto
                {
                    AccountId = row.RecipientAccountId
                } : MapAccount(recipient),
                Status = row.Status,
                CreatedAt = row.CreatedAt,
                ProcessedAt = row.ProcessedAt,
                Version = row.Version
            });
        }
        return new GetFriendRequestsResponse
        {
            Data = page
        };
    }

    public async Task<GetAccountBlocksResponse> GetAccountBlocksAsync(GetAccountBlocksRequest message,
        CancellationToken cancellationToken)
    {
        var rows = await _data.GetBlocksAsync(AccountId, message.PageIndex, message.PageSize, cancellationToken).ConfigureAwait(false);
        var page = new SocialPageDto<SocialAccountDto>
        {
            PageIndex = message.PageIndex,
            PageSize = message.PageSize,
            TotalCount = rows.TotalCount
        };
        var accounts = await _data.GetAccountsAsync(rows.Items.Select(value => value.BlockedAccountId).ToArray(), cancellationToken).ConfigureAwait(false);
        foreach (var row in rows.Items)
        {
            accounts.TryGetValue(row.BlockedAccountId, out var account);
            if (account != null)
            {
                page.Items.Add(MapAccount(account));
            }
        }
        return new GetAccountBlocksResponse
        {
            Data = page
        };
    }

    public async Task<SendFriendRequestResponse> SendFriendRequestAsync(SendFriendRequestCommand message,
        CancellationToken cancellationToken)
    {
        if (message.TargetAccountId == AccountId)
        {
            return Error<SendFriendRequestResponse>(HttpStatusCode.BadRequest, "social.self_operation", text => text.SelfOperation);
        }
        if (!await _data.LockAccountsAsync(AccountId, message.TargetAccountId, cancellationToken).ConfigureAwait(false))
        {
            return Error<SendFriendRequestResponse>(HttpStatusCode.NotFound, "social.account_not_found", text => text.AccountNotFound);
        }
        var receipt = await _data.GetRequestReceiptAsync(AccountId, message.RequestId, cancellationToken).ConfigureAwait(false);
        if (receipt != null)
        {
            if (receipt.RecipientAccountId != message.TargetAccountId)
            {
                return Error<SendFriendRequestResponse>(HttpStatusCode.Conflict, "social.idempotency_conflict", text => text.IdempotencyConflict);
            }
            return new SendFriendRequestResponse
            {
                Data = await MapRequestAsync(receipt, cancellationToken).ConfigureAwait(false)
            };
        }
        if (await _data.IsBlockedAsync(AccountId, message.TargetAccountId, cancellationToken).ConfigureAwait(false))
        {
            return Error<SendFriendRequestResponse>(HttpStatusCode.Forbidden, "social.blocked", text => text.Blocked);
        }
        if (await _data.AreFriendsAsync(AccountId, message.TargetAccountId, cancellationToken).ConfigureAwait(false))
        {
            return Error<SendFriendRequestResponse>(HttpStatusCode.Conflict, "social.already_friends", text => text.AlreadyFriends);
        }
        var pending = await _data.GetPendingPairAsync(AccountId, message.TargetAccountId, cancellationToken).ConfigureAwait(false);
        var own = pending.SingleOrDefault(value => value.SenderAccountId == AccountId);
        if (own != null)
        {
            return new SendFriendRequestResponse
            {
                Data = await MapRequestAsync(own, cancellationToken).ConfigureAwait(false)
            };
        }
        if (pending.Count > 0)
        {
            return Error<SendFriendRequestResponse>(HttpStatusCode.Conflict, "social.incoming_request", text => text.IncomingRequest);
        }
        if (await _data.CountPendingAsync(AccountId, cancellationToken).ConfigureAwait(false) >= PendingRequestLimit ||
            await _data.CountPendingAsync(message.TargetAccountId, cancellationToken).ConfigureAwait(false) >= PendingRequestLimit)
        {
            return Error<SendFriendRequestResponse>(HttpStatusCode.Conflict, "social.limit_reached", text => text.LimitReached);
        }
        var request = new FriendRequest
        {
            Id = Guid.NewGuid(),
            SenderAccountId = AccountId,
            RecipientAccountId = message.TargetAccountId,
            RequestId = message.RequestId,
            Status = FriendRequestStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _data.AddRequestAsync(request, cancellationToken).ConfigureAwait(false);
        await NotifyRequestAsync(request, cancellationToken).ConfigureAwait(false);
        return new SendFriendRequestResponse
        {
            Data = await MapRequestAsync(request, cancellationToken).ConfigureAwait(false)
        };
    }

    private async Task<VoidBotResponse<bool>> ProcessRequestAsync(Guid id, FriendRequestStatus status,
        CancellationToken cancellationToken)
    {
        var request = await _data.GetRequestAsync(id, cancellationToken).ConfigureAwait(false);
        if (request == null || (status == FriendRequestStatus.Withdrawn ? request.SenderAccountId : request.RecipientAccountId) != AccountId)
        {
            return Error<VoidBotResponse<bool>>(HttpStatusCode.NotFound, "social.request_not_found", text => text.RequestNotFound);
        }
        if (!await _data.LockAccountsAsync(request.SenderAccountId, request.RecipientAccountId, cancellationToken).ConfigureAwait(false))
        {
            return Error<VoidBotResponse<bool>>(HttpStatusCode.NotFound, "social.account_not_found", text => text.AccountNotFound);
        }
        // 锁前查询只用于确定双方；锁后必须重新读取，不能依赖 EF 已跟踪的旧状态。
        request = await _data.LockRequestAsync(id, cancellationToken).ConfigureAwait(false);
        if (request!.Status == status)
        {
            return new VoidBotResponse<bool>
            {
                Data = true
            };
        }
        if (request.Status != FriendRequestStatus.Pending)
        {
            return Error<VoidBotResponse<bool>>(HttpStatusCode.Conflict, "social.request_processed", text => text.RequestProcessed);
        }
        if (status == FriendRequestStatus.Accepted)
        {
            if (await _data.IsBlockedAsync(request.SenderAccountId, request.RecipientAccountId, cancellationToken).ConfigureAwait(false))
            {
                return Error<VoidBotResponse<bool>>(HttpStatusCode.Forbidden, "social.blocked", text => text.Blocked);
            }
            if (await _data.CountFriendsAsync(request.SenderAccountId, cancellationToken).ConfigureAwait(false) >= FriendLimit ||
                await _data.CountFriendsAsync(request.RecipientAccountId, cancellationToken).ConfigureAwait(false) >= FriendLimit)
            {
                return Error<VoidBotResponse<bool>>(HttpStatusCode.Conflict, "social.limit_reached", text => text.LimitReached);
            }
            var ordered = new[] { request.SenderAccountId, request.RecipientAccountId }
                .OrderBy(value => value.ToString("D"), StringComparer.Ordinal).ToArray();
            if (!await _data.AreFriendsAsync(ordered[0], ordered[1], cancellationToken).ConfigureAwait(false))
            {
                await _data.AddFriendshipAsync(new Friendship
                {
                    Id = Guid.NewGuid(),
                    FirstAccountId = ordered[0],
                    SecondAccountId = ordered[1],
                    CreatedAt = DateTimeOffset.UtcNow
                }, cancellationToken).ConfigureAwait(false);
            }
            await _notifications.EnqueueAsync(ordered, "social.friendship_changed", request.Id,
                request.Version + 1, request.RequestId, cancellationToken).ConfigureAwait(false);
        }
        request.Status = status;
        request.ProcessedAt = DateTimeOffset.UtcNow;
        request.Version++;
        await NotifyRequestAsync(request, cancellationToken).ConfigureAwait(false);
        return new VoidBotResponse<bool>
        {
            Data = true
        };
    }

    public async Task<AcceptFriendRequestResponse> AcceptFriendRequestAsync(AcceptFriendRequestCommand message, CancellationToken cancellationToken)
    {
        var result = await ProcessRequestAsync(message.FriendRequestId, FriendRequestStatus.Accepted, cancellationToken).ConfigureAwait(false);
        return new AcceptFriendRequestResponse
        {
            Code = result.Code,
            ErrorCode = result.ErrorCode,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<RejectFriendRequestResponse> RejectFriendRequestAsync(RejectFriendRequestCommand message, CancellationToken cancellationToken)
    {
        var result = await ProcessRequestAsync(message.FriendRequestId, FriendRequestStatus.Rejected, cancellationToken).ConfigureAwait(false);
        return new RejectFriendRequestResponse
        {
            Code = result.Code,
            ErrorCode = result.ErrorCode,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<WithdrawFriendRequestResponse> WithdrawFriendRequestAsync(WithdrawFriendRequestCommand message, CancellationToken cancellationToken)
    {
        var result = await ProcessRequestAsync(message.FriendRequestId, FriendRequestStatus.Withdrawn, cancellationToken).ConfigureAwait(false);
        return new WithdrawFriendRequestResponse
        {
            Code = result.Code,
            ErrorCode = result.ErrorCode,
            Message = result.Message,
            Data = result.Data
        };
    }

    private async Task<VoidBotResponse<bool>> ChangeRelationshipAsync(Guid target, bool block, bool unblock,
        CancellationToken cancellationToken)
    {
        if (target == AccountId)
        {
            return Error<VoidBotResponse<bool>>(HttpStatusCode.BadRequest, "social.self_operation", text => text.SelfOperation);
        }
        if (!await _data.LockAccountsAsync(AccountId, target, cancellationToken).ConfigureAwait(false))
        {
            return Error<VoidBotResponse<bool>>(HttpStatusCode.NotFound, "social.account_not_found", text => text.AccountNotFound);
        }
        var existingBlock = await _data.GetBlockAsync(AccountId, target, cancellationToken).ConfigureAwait(false);
        if (unblock)
        {
            if (existingBlock != null)
            {
                _data.RemoveBlock(existingBlock);
                await _notifications.EnqueueAsync(new[] { AccountId }, "social.block_list_changed", target,
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Guid.NewGuid(), cancellationToken).ConfigureAwait(false);
            }
            return new VoidBotResponse<bool>
            {
                Data = true
            };
        }
        var friendship = await _data.GetFriendshipAsync(AccountId, target, cancellationToken).ConfigureAwait(false);
        if (friendship != null)
        {
            _data.RemoveFriendship(friendship);
        }
        if (block && existingBlock == null)
        {
            await _data.AddBlockAsync(new AccountBlock
            {
                Id = Guid.NewGuid(),
                AccountId = AccountId,
                BlockedAccountId = target,
                CreatedAt = DateTimeOffset.UtcNow
            }, cancellationToken).ConfigureAwait(false);
            await _notifications.EnqueueAsync(new[] { AccountId }, "social.block_list_changed", target,
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Guid.NewGuid(), cancellationToken).ConfigureAwait(false);
        }
        var pending = await _data.GetPendingPairAsync(AccountId, target, cancellationToken).ConfigureAwait(false);
        foreach (var request in pending)
        {
            request.Status = FriendRequestStatus.Cancelled;
            request.ProcessedAt = DateTimeOffset.UtcNow;
            request.Version++;
            await NotifyRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }
        await _notifications.EnqueueAsync(new[] { AccountId, target }, "social.friendship_changed",
            target, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), Guid.NewGuid(), cancellationToken).ConfigureAwait(false);
        return new VoidBotResponse<bool>
        {
            Data = true
        };
    }

    public async Task<RemoveFriendResponse> RemoveFriendAsync(RemoveFriendCommand message, CancellationToken cancellationToken)
    {
        var result = await ChangeRelationshipAsync(message.TargetAccountId, false, false, cancellationToken).ConfigureAwait(false);
        return new RemoveFriendResponse
        {
            Code = result.Code,
            ErrorCode = result.ErrorCode,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<BlockAccountResponse> BlockAccountAsync(BlockAccountCommand message, CancellationToken cancellationToken)
    {
        var result = await ChangeRelationshipAsync(message.TargetAccountId, true, false, cancellationToken).ConfigureAwait(false);
        return new BlockAccountResponse
        {
            Code = result.Code,
            ErrorCode = result.ErrorCode,
            Message = result.Message,
            Data = result.Data
        };
    }

    public async Task<UnblockAccountResponse> UnblockAccountAsync(UnblockAccountCommand message, CancellationToken cancellationToken)
    {
        var result = await ChangeRelationshipAsync(message.TargetAccountId, false, true, cancellationToken).ConfigureAwait(false);
        return new UnblockAccountResponse
        {
            Code = result.Code,
            ErrorCode = result.ErrorCode,
            Message = result.Message,
            Data = result.Data
        };
    }
}
