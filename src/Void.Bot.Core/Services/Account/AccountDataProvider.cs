using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Core.Services.Account;

public interface IAccountDataProvider : IScope
{
    Task<UserAccountDto?> GetUserAccountAsync(
        Guid? userId, string displayName, CancellationToken cancellationToken);
    
    Task<UserAccountDto?> GetUserAccountByThirdPartyAsync(
        string provider, string thirdPartyUserId, CancellationToken cancellationToken);
    
    Task<UserThirdPartyDto?> GetThirdPartyByThirdPartyAsync(
        string currentProvider, string thirdPartyUserId, string targetProvider, CancellationToken cancellationToken);
    
    Task<UserThirdPartyDto?> GetThirdPartyByUserIdAsync(
        Guid userId, string provider, CancellationToken cancellationToken);
    
    Task<UserAccountDto?> RegisterUserAccountAsync(
        string name, string encryptionPassword, CancellationToken cancellationToken);
    
    Task LinkThirdPartyAsync(UserThirdPartyDto thirdParty, CancellationToken cancellationToken);
    
    Task UpdateThirdPartyAsync(UserThirdPartyDto thirdParty, CancellationToken cancellationToken);

    Task UpdateUserAccountAsync(UserAccountDto userAccountDto, CancellationToken cancellationToken);
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
    
    public async Task<UserAccountDto?> GetUserAccountAsync(
        Guid? userId, string displayName, CancellationToken cancellationToken)
    {
        var (name, suffix) = displayName.Split('#') switch
        {
            [var n, var s] => (n, s),
            _ => (null, null)
        };

        if (userId == null && string.IsNullOrEmpty(name) && string.IsNullOrEmpty(suffix))
        {
            return null;
        }
        
        var user = await _repository.QueryNoTracking<UserAccount>()
            .WhereIF(userId.HasValue, x => x.Id == userId)
            .WhereIF(!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(suffix), x => x.Name.Equals(name) && x.Suffix.Equals(suffix))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        
        if (user != null)
        {
            user.ThirdParties = await _repository.QueryNoTracking<UserThirdParty>(x => x.UserId == user.Id)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        
        return _mapper.Map<UserAccountDto>(user);
    }

    public async Task<UserAccountDto?> GetUserAccountByThirdPartyAsync(
        string provider, string thirdPartyUserId, CancellationToken cancellationToken)
    {
        UserAccountDto? user = null;
        
        var thirdParty = await _repository
            .QueryNoTracking<UserThirdParty>(x => x.Provider.Equals(provider) && x.ThirdPartyUserId.Equals(thirdPartyUserId) )
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        
        if (thirdParty != null)
            user = await GetUserAccountAsync(thirdParty.UserId, string.Empty, cancellationToken).ConfigureAwait(false);

        return user;
    }

    public async Task<UserThirdPartyDto?> GetThirdPartyByThirdPartyAsync(
        string currentProvider, string thirdPartyUserId, string targetProvider, CancellationToken cancellationToken)
    {
        var current = await _repository
            .QueryNoTracking<UserThirdParty>(x => x.Provider.Equals(currentProvider) && x.ThirdPartyUserId.Equals(thirdPartyUserId))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (current == null)
        {
            return null;
        }
        
        var target = await _repository
            .QueryNoTracking<UserThirdParty>(x => x.UserId.Equals(current.UserId) && x.Provider.Equals(targetProvider))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<UserThirdPartyDto>(target);
    }

    public async Task<UserThirdPartyDto?> GetThirdPartyByUserIdAsync(
        Guid userId, string provider, CancellationToken cancellationToken)
    {
        var thirdParty = await _repository
            .QueryNoTracking<UserThirdParty>(x => x.UserId.Equals(userId) && x.Provider.Equals(provider))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        
        return _mapper.Map<UserThirdPartyDto>(thirdParty);
    }

    public async Task<UserAccountDto?> RegisterUserAccountAsync(
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
        
        return _mapper.Map<UserAccountDto>(user);
    }

    public async Task LinkThirdPartyAsync(UserThirdPartyDto thirdParty, CancellationToken cancellationToken)
    {
        var item = _mapper.Map<UserThirdParty>(thirdParty);

        await _repository.InsertAsync(item, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateThirdPartyAsync(UserThirdPartyDto thirdParty, CancellationToken cancellationToken)
    {
        var item = _mapper.Map<UserThirdParty>(thirdParty);
        
        await _repository.UpdateAsync(item, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateUserAccountAsync(UserAccountDto userAccountDto, CancellationToken cancellationToken)
    {
        var item = _mapper.Map<UserAccount>(userAccountDto);

        await _repository.UpdateAsync(item, cancellationToken).ConfigureAwait(false);
    }
}