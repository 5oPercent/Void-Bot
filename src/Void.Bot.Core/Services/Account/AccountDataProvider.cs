using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Core.Services.Account;

public interface IAccountDataProvider : IScope
{
    Task<UserAccountInfoDto?> GetUserAccountAsync(
        Guid? userId, string? displayName, CancellationToken cancellationToken);
    
    Task<UserAccountInfoDto?> GetUserAccountByThirdPartyAsync(
        string provider, string thirdPartyUserId, CancellationToken cancellationToken);
    
    Task<UserAccountInfoDto?> RegisterUserAccountAsync(
        string name, string encryptionPassword, CancellationToken cancellationToken);
    
    Task LinkThirdPartyAsync(UserThirdPartyDto thirdParty, CancellationToken cancellationToken);
}

public class AccountDataProvider : IAccountDataProvider
{
    private readonly IMapper _mapper;
    private readonly IRepository _repository;

    public AccountDataProvider(IMapper mapper, IRepository repository)
    {
        _mapper = mapper;
        _repository = repository;
    }
    
    public async Task<UserAccountInfoDto?> GetUserAccountAsync(
        Guid? userId, string? displayName, CancellationToken cancellationToken)
    {
        var (name, suffix) = displayName?.Split('#') switch
        {
            [var n, var s] => (n, s),
            _ => (null, null)
        };
        
        var user = await _repository.QueryNoTracking<UserAccount>()
            .WhereIF(userId.HasValue, x => x.Id == userId)
            .WhereIF(!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(suffix), x => x.Name.Equals(name) && x.Suffix.Equals(suffix))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        
        if (user != null)
        {
            user.ThirdParties = await _repository.QueryNoTracking<UserThirdParty>(x => x.UserId == user.Id)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        
        return _mapper.Map<UserAccountInfoDto>(user);
    }

    public async Task<UserAccountInfoDto?> GetUserAccountByNameSuffixAsync(
        string name, string suffix, CancellationToken cancellationToken)
    {
        var user = await _repository.QueryNoTracking<UserAccount>()
            .Where(x => x.Name.Equals(name) && x.Suffix.Equals(suffix))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        
        if (user != null)
        {
            user.ThirdParties = await _repository.QueryNoTracking<UserThirdParty>(x => x.UserId == user.Id)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        
        return _mapper.Map<UserAccountInfoDto>(user);
    }

    public async Task<UserAccountInfoDto?> GetUserAccountByThirdPartyAsync(
        string provider, string thirdPartyUserId, CancellationToken cancellationToken)
    {
        UserAccountInfoDto? user = null;
        
        var thirdParty = await _repository
            .QueryNoTracking<UserThirdParty>(x => x.Provider.Equals(provider) && x.ThirdPartyUserId.Equals(thirdPartyUserId) )
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        
        if (thirdParty != null)
            user = await GetUserAccountAsync(thirdParty.UserId, string.Empty, cancellationToken).ConfigureAwait(false);

        return user;
    }

    public async Task<UserAccountInfoDto?> RegisterUserAccountAsync(
        string name, string encryptionPassword, CancellationToken cancellationToken)
    {
        var accountCount = await _repository.QueryNoTracking<UserAccount>(x => x.Name == name)
            .CountAsync(cancellationToken).ConfigureAwait(false);
        if (accountCount >= 100)
        {
            return null;
        }
        
        var user = new UserAccount
        {
            Name = name,
            Password = encryptionPassword
        };

        var isExist = true;
        while (isExist)
        {
            isExist = await _repository.QueryNoTracking<UserAccount>(x => x.Name == name && x.Suffix == user.Suffix)
                .AnyAsync(cancellationToken).ConfigureAwait(false);

            if (isExist)
            {
                user.Suffix = "".GenerateNumberSuffix();
            }
        }
        
        await _repository.InsertAsync(user, cancellationToken).ConfigureAwait(false);
        
        return _mapper.Map<UserAccountInfoDto>(user);
    }

    public async Task LinkThirdPartyAsync(UserThirdPartyDto thirdParty, CancellationToken cancellationToken)
    {
        var item = _mapper.Map<UserThirdParty>(thirdParty);

        await _repository.InsertAsync(item, cancellationToken).ConfigureAwait(false);
    }
}