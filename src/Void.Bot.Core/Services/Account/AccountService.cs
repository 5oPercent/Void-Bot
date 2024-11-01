using AutoMapper;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Services.Account;

public interface IAccountService : IScope
{
    Task<GetUserAccountResponse> GetUserAccountAsync(
        GetUserAccountRequest request, CancellationToken cancellationToken);
    
    Task<GetUserAccountByThirdPartyResponse> GetUserAccountByThirdPartyAsync(
        GetUserAccountByThirdPartyRequest request, CancellationToken cancellationToken);
    
    Task<RegisterUserAccountResponse> RegisterUserAccountAsync(
        RegisterUserAccountCommand command, CancellationToken cancellationToken);
    
    Task<LinkUserAccountFromThirdPartyResponse> LinkAccountFromThirdPartyAsync(
        LinkUserAccountFromThirdPartyCommand command, CancellationToken cancellationToken);
}

public class AccountService : IAccountService
{
    private readonly IMapper _mapper;
    private readonly IAccountDataProvider _dataProvider;

    public AccountService(IMapper mapper, IAccountDataProvider dataProvider)
    {
        _mapper = mapper;
        _dataProvider = dataProvider;
    }
    
    public async Task<GetUserAccountResponse> GetUserAccountAsync(
        GetUserAccountRequest request, CancellationToken cancellationToken)
    {
        var userInfo = await _dataProvider.GetUserAccountAsync(
            request.UserId, request.DisplayName, cancellationToken).ConfigureAwait(false);
        
        return new GetUserAccountResponse
        {
            Data = userInfo
        };
    }

    public async Task<GetUserAccountByThirdPartyResponse> GetUserAccountByThirdPartyAsync(
        GetUserAccountByThirdPartyRequest request, CancellationToken cancellationToken)
    {
        var userInfo = await _dataProvider.GetUserAccountByThirdPartyAsync(
            request.Platform.ToString(), request.ThirdPartyUserId, cancellationToken).ConfigureAwait(false);

        return new GetUserAccountByThirdPartyResponse
        {
            Data = userInfo
        };
    }

    public async Task<RegisterUserAccountResponse> RegisterUserAccountAsync(
        RegisterUserAccountCommand command, CancellationToken cancellationToken)
    {
        var userInfo = await _dataProvider.RegisterUserAccountAsync(
            command.Name, command.Password.Md5Encrypt(), cancellationToken).ConfigureAwait(false);

        return new RegisterUserAccountResponse
        {
            Data = userInfo
        };
    }
    
    public async Task<LinkUserAccountFromThirdPartyResponse> LinkAccountFromThirdPartyAsync(
        LinkUserAccountFromThirdPartyCommand command, CancellationToken cancellationToken)
    {
        var getUserByThirdPartyResponse = await GetUserAccountByThirdPartyAsync(
            new GetUserAccountByThirdPartyRequest
            {
                Platform = command.Authorization.Platform,
                ThirdPartyUserId = command.Authorization.AccountId
            }, cancellationToken).ConfigureAwait(false);

        if (getUserByThirdPartyResponse.Data != null)
        {
            throw new Exception($"当前{AppPlatformEnum.Void}账号已关联过{command.Authorization.Platform.ToString()}平台.");
        }
        
        UserAccountInfoDto userInfo;

        if (string.IsNullOrEmpty(command.MasterDisplayName))
        {
            var registerResponse = await RegisterUserAccountAsync(new RegisterUserAccountCommand
            {
                Name = command.Authorization.AccountName,
                Password = "".GenerateRandomString()
            }, cancellationToken).ConfigureAwait(false);

            userInfo = registerResponse.Data ?? throw new Exception("创建Void账号失败.");
        }
        else
        {
            var getUserAccountResponse = await GetUserAccountAsync(
                new GetUserAccountRequest
                {
                    DisplayName = command.MasterDisplayName
                }, cancellationToken).ConfigureAwait(false);

            userInfo = getUserAccountResponse.Data ?? throw new Exception("未找到对应Void账号.");
        }
        
        var thirdParty = new UserThirdPartyDto
        {
            Id = Guid.NewGuid(),
            UserId = userInfo.Id,
            Provider = command.Authorization.Platform.ToString(),
            ThirdPartyUserId = command.Authorization.AccountId,
            AccessToken = command.AccessToken,
            RefreshToken = command.RefreshToken,
            ExpiresAt = command.ExpiresAt,
            ExtraData = command.ExtraData
        };
        
        await _dataProvider.LinkThirdPartyAsync(thirdParty, cancellationToken).ConfigureAwait(false);
        
        return new LinkUserAccountFromThirdPartyResponse();
    }
}