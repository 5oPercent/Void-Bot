using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Constants.Security;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Core.Services.Account;

public interface IAccountDataProvider : IScope
{
    Task<UserAccountDto?> GetUserAccountAsync(
        Guid? userId, string displayName, CancellationToken cancellationToken);

    Task<PagedListDto<UserAccountDto>> GetUserAccountPageAsync(
        List<Guid>? userIds = null, int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    
    Task<UserAccountDto?> GetUserAccountByThirdPartyAsync(
        string provider, string thirdPartyUserId, CancellationToken cancellationToken);

    Task<UserAccountDto?> GetUserAccountByEmailAsync(
        string email, CancellationToken cancellationToken);

    Task<bool> IsEmailAvailableAsync(
        string email, Guid? excludedUserId, CancellationToken cancellationToken);
    
    Task<UserThirdPartyDto?> GetThirdPartyByThirdPartyAsync(
        string currentProvider, string thirdPartyUserId, string targetProvider, CancellationToken cancellationToken);
    
    Task<UserThirdPartyDto?> GetThirdPartyByUserIdAsync(
        Guid userId, string provider, CancellationToken cancellationToken);

    Task<UserThirdPartyDto?> GetThirdPartyBindingAsync(
        string provider, string thirdPartyUserId, CancellationToken cancellationToken);
    
    Task<UserAccountDto?> RegisterUserAccountAsync(
        string name,
        string encryptionPassword,
        string? email,
        DateTimeOffset? emailVerifiedAt,
        CancellationToken cancellationToken);

    Task<bool> CanChangeUserAccountNameAsync(
        Guid userId, string name, CancellationToken cancellationToken);

    Task<UserAccountDto?> ChangeUserAccountNameAsync(
        Guid userId, string name, CancellationToken cancellationToken);
    
    Task LinkThirdPartyAsync(UserThirdPartyDto thirdParty, CancellationToken cancellationToken);
    
    Task UpdateThirdPartyAsync(UserThirdPartyDto thirdParty, CancellationToken cancellationToken);

    Task DeleteThirdPartyAsync(
        Guid userId, string provider, CancellationToken cancellationToken);

    Task UpdateUserAccountAsync(UserAccountDto userAccountDto, CancellationToken cancellationToken);
}

public class AccountDataProvider : IAccountDataProvider
{
    private readonly IMapper _mapper;
    private readonly VoidBotDbContext _dbContext;
    private readonly IRepository<UserAccount> _userAccountRep;
    private readonly IRepository<UserThirdParty> _userThirdPartyRep;
    private readonly IRepository<UserRole> _userRoleRep;

    public AccountDataProvider(
        IMapper mapper,
        VoidBotDbContext dbContext,
        IRepository<UserAccount> userAccountRep,
        IRepository<UserThirdParty> userThirdPartyRep,
        IRepository<UserRole> userRoleRep)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _userAccountRep = userAccountRep;
        _userThirdPartyRep = userThirdPartyRep;
        _userRoleRep = userRoleRep;
    }
    
    public async Task<UserAccountDto?> GetUserAccountAsync(
        Guid? userId, string displayName, CancellationToken cancellationToken)
    {
        string? name = null;
        string? suffix = null;
        var separatorIndex = displayName.LastIndexOf('#');
        if (separatorIndex > 0 && separatorIndex < displayName.Length - 1)
        {
            name = displayName[..separatorIndex];
            suffix = displayName[(separatorIndex + 1)..];
        }

        if (userId == null && string.IsNullOrEmpty(name) && string.IsNullOrEmpty(suffix))
        {
            return null;
        }
        
        var user = await _userAccountRep.QueryNoTracking()
            .WhereIF(userId.HasValue, x => x.Id == userId)
            .WhereIF(!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(suffix), x => x.Name.Equals(name) && x.Suffix.Equals(suffix))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        
        if (user != null)
        {
            user.ThirdParties = await _userThirdPartyRep.QueryNoTracking(x => x.UserId == user.Id)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }
        
        return _mapper.Map<UserAccountDto>(user);
    }

    public async Task<PagedListDto<UserAccountDto>> GetUserAccountPageAsync(
        List<Guid>? userIds = null, int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _userAccountRep.QueryNoTracking();
        
        if (userIds is { Count: > 0 })
        {
            var userIdSet = new HashSet<Guid>(userIds);
            query = query.Where(x => userIdSet.Contains(x.Id));
        }
        
        var result = await query.ToPagedAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);
        
        return _mapper.Map<PagedListDto<UserAccountDto>>(result);
    }

    public async Task<UserAccountDto?> GetUserAccountByThirdPartyAsync(
        string provider, string thirdPartyUserId, CancellationToken cancellationToken)
    {
        UserAccountDto? user = null;
        
        var thirdParty = await _userThirdPartyRep
            .QueryNoTracking(x => x.Provider.Equals(provider) && x.ThirdPartyUserId.Equals(thirdPartyUserId) )
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        
        if (thirdParty != null)
            user = await GetUserAccountAsync(thirdParty.UserId, string.Empty, cancellationToken).ConfigureAwait(false);

        return user;
    }

    public async Task<UserAccountDto?> GetUserAccountByEmailAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var user = await _userAccountRep.QueryNoTracking(x => x.Email == email)
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        return _mapper.Map<UserAccountDto>(user);
    }

    public async Task<bool> IsEmailAvailableAsync(
        string email,
        Guid? excludedUserId,
        CancellationToken cancellationToken)
    {
        return !await _userAccountRep.QueryNoTracking()
            .WhereIF(excludedUserId.HasValue, x => x.Id != excludedUserId)
            .AnyAsync(x => x.Email == email, cancellationToken).ConfigureAwait(false);
    }

    public async Task<UserThirdPartyDto?> GetThirdPartyByThirdPartyAsync(
        string currentProvider, string thirdPartyUserId, string targetProvider, CancellationToken cancellationToken)
    {
        var current = await _userThirdPartyRep
            .QueryNoTracking(x => x.Provider.Equals(currentProvider) && x.ThirdPartyUserId.Equals(thirdPartyUserId))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (current == null)
        {
            return null;
        }
        
        var target = await _userThirdPartyRep
            .QueryNoTracking(x => x.UserId.Equals(current.UserId) && x.Provider.Equals(targetProvider))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<UserThirdPartyDto>(target);
    }

    public async Task<UserThirdPartyDto?> GetThirdPartyByUserIdAsync(
        Guid userId, string provider, CancellationToken cancellationToken)
    {
        var thirdParty = await _userThirdPartyRep
            .QueryNoTracking(x => x.UserId.Equals(userId) && x.Provider.Equals(provider))
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        
        return _mapper.Map<UserThirdPartyDto>(thirdParty);
    }

    public async Task<UserThirdPartyDto?> GetThirdPartyBindingAsync(
        string provider, string thirdPartyUserId, CancellationToken cancellationToken)
    {
        var thirdParty = await _userThirdPartyRep
            .QueryNoTracking(x => x.Provider == provider && x.ThirdPartyUserId == thirdPartyUserId)
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        return _mapper.Map<UserThirdPartyDto>(thirdParty);
    }

    public async Task<UserAccountDto?> RegisterUserAccountAsync(
        string name,
        string encryptionPassword,
        string? email,
        DateTimeOffset? emailVerifiedAt,
        CancellationToken cancellationToken)
    {
        var accountCount = await _userAccountRep.QueryNoTracking(x => x.Name == name)
            .CountAsync(cancellationToken).ConfigureAwait(false);
        if (accountCount >= 100)
        {
            return null;
        }
        
        var user = new UserAccount
        {
            Name = name,
            Password = encryptionPassword,
            Email = email,
            EmailVerifiedAt = emailVerifiedAt
        };

        var isExist = true;
        while (isExist)
        {
            isExist = await _userAccountRep.QueryNoTracking(x => x.Name == name && x.Suffix == user.Suffix)
                .AnyAsync(cancellationToken).ConfigureAwait(false);

            if (isExist)
            {
                user.Suffix = "".GenerateNumberSuffix();
            }
        }
        
        var isFirstUser = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT IGNORE INTO `security_bootstrap_claim` (`claim_key`, `user_id`, `created_at`)
             SELECT {RoleConstants.SuperAdminBootstrapClaim}, {user.Id}, CURRENT_TIMESTAMP
             WHERE NOT EXISTS (SELECT 1 FROM `user_account` LIMIT 1);
             """, cancellationToken).ConfigureAwait(false) == 1;

        await _userAccountRep.InsertAsync(user, cancellationToken).ConfigureAwait(false);

        if (isFirstUser)
        {
            await _userRoleRep.InsertAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = RoleConstants.SuperAdminId
            }, cancellationToken).ConfigureAwait(false);
        }
        
        return _mapper.Map<UserAccountDto>(user);
    }

    public async Task<bool> CanChangeUserAccountNameAsync(
        Guid userId, string name, CancellationToken cancellationToken)
    {
        if (!await _userAccountRep.AnyAsync(x => x.Id == userId, cancellationToken).ConfigureAwait(false))
        {
            return false;
        }

        var accountCount = await _userAccountRep.CountAsync(
            x => x.Id != userId && x.Name == name, cancellationToken).ConfigureAwait(false);

        return accountCount < 100;
    }

    public async Task<UserAccountDto?> ChangeUserAccountNameAsync(
        Guid userId, string name, CancellationToken cancellationToken)
    {
        var user = await _userAccountRep.SingleOrDefaultAsync(
            x => x.Id == userId, cancellationToken).ConfigureAwait(false);
        if (user == null)
        {
            return null;
        }

        var accountCount = await _userAccountRep.CountAsync(
            x => x.Id != userId && x.Name == name, cancellationToken).ConfigureAwait(false);
        if (accountCount >= 100)
        {
            return null;
        }

        var originalSuffix = user.Suffix;
        var suffix = string.Empty;
        var isExist = true;
        while (suffix == originalSuffix || isExist)
        {
            suffix = "".GenerateNumberSuffix();
            isExist = await _userAccountRep.AnyAsync(
                x => x.Id != userId && x.Name == name && x.Suffix == suffix,
                cancellationToken).ConfigureAwait(false);
        }

        user.Name = name;
        user.Suffix = suffix;
        await _userAccountRep.UpdateAsync(user, cancellationToken).ConfigureAwait(false);

        return _mapper.Map<UserAccountDto>(user);
    }

    public async Task LinkThirdPartyAsync(UserThirdPartyDto thirdParty, CancellationToken cancellationToken)
    {
        var item = _mapper.Map<UserThirdParty>(thirdParty);

        await _userThirdPartyRep.InsertAsync(item, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateThirdPartyAsync(UserThirdPartyDto thirdParty, CancellationToken cancellationToken)
    {
        var item = _mapper.Map<UserThirdParty>(thirdParty);
        
        await _userThirdPartyRep.UpdateAsync(item, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteThirdPartyAsync(
        Guid userId, string provider, CancellationToken cancellationToken)
    {
        return _userThirdPartyRep.DeleteAsync(
            x => x.UserId == userId && x.Provider == provider,
            cancellationToken);
    }

    public async Task UpdateUserAccountAsync(UserAccountDto userAccountDto, CancellationToken cancellationToken)
    {
        var item = _mapper.Map<UserAccount>(userAccountDto);

        await _userAccountRep.UpdateAsync(item, cancellationToken).ConfigureAwait(false);
    }
}
