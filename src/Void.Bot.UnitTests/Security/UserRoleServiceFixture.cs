using Shouldly;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Commands.Security;
using Void.Bot.Message.Constants.Security;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Security;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Security;
using NSubstitute;
using System.Linq.Expressions;

namespace Void.Bot.UnitTest.Security;

// Helper class that wraps a list and provides IQueryable and IAsyncEnumerable support
internal class AsyncQueryableWrapper<T> : IQueryable<T>, IAsyncEnumerable<T>
{
    private readonly List<T> _items;
    private readonly SimpleQueryProvider<T> _provider;

    public AsyncQueryableWrapper(IEnumerable<T> items)
    {
        _items = items.ToList();
        _provider = new SimpleQueryProvider<T>(_items);
    }

    public AsyncQueryableWrapper(List<T> items, SimpleQueryProvider<T> provider)
    {
        _items = items;
        _provider = provider;
    }

    public Type ElementType => typeof(T);

    public Expression Expression => Expression.Constant(this);

    public IQueryProvider Provider => _provider;

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumeratorWrapper<T>(_items.GetEnumerator());
    }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    internal List<T> Items => _items;
}

internal class SimpleQueryProvider<T> : IQueryProvider
{
    private readonly List<T> _items;

    public SimpleQueryProvider(List<T> items)
    {
        _items = items;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return CreateQuery<T>(expression);
    }

    public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
    {
        if (expression is not MethodCallExpression methodCall)
        {
            return new AsyncQueryableWrapper<TResult>(Array.Empty<TResult>());
        }

        // Get source items from expression tree
        var sourceItems = GetSourceItems<T>(expression);

        switch (methodCall.Method.Name)
        {
            case "Select":
            {
                var quoteExpr = methodCall.Arguments[1] as UnaryExpression;
                var selector = (quoteExpr?.Operand as LambdaExpression)?.Compile();

                if (selector != null && sourceItems != null)
                {
                    var selected = sourceItems
                        .Select(x => selector.DynamicInvoke(x))
                        .Where(x => x != null)
                        .Cast<TResult>()
                        .ToList();
                    return new AsyncQueryableWrapper<TResult>(selected);
                }
                return new AsyncQueryableWrapper<TResult>(Array.Empty<TResult>());
            }

            case "Where":
            {
                var quoteExpr = methodCall.Arguments[1] as UnaryExpression;
                var predicate = (quoteExpr?.Operand as LambdaExpression)?.Compile() as Func<T, bool>;

                if (predicate != null && sourceItems != null)
                {
                    var filtered = sourceItems.Cast<T>().Where(predicate).ToList();
                    // When TResult is same as T, we can pass the list directly
                    if (typeof(TResult) == typeof(T))
                    {
                        return new AsyncQueryableWrapper<TResult>((IEnumerable<TResult>)(object)filtered);
                    }
                }
                return new AsyncQueryableWrapper<TResult>(Array.Empty<TResult>());
            }

            default:
                return new AsyncQueryableWrapper<TResult>(Array.Empty<TResult>());
        }
    }

    private List<object>? GetSourceItems<TSource>(Expression expression)
    {
        if (expression is MethodCallExpression methodCall)
        {
            if (methodCall.Method.Name == "Select" || methodCall.Method.Name == "Where")
            {
                var sourceExpr = methodCall.Arguments[0];

                // If source is another operation, recursively get its items
                if (sourceExpr is MethodCallExpression innerCall)
                {
                    return GetSourceItems<TSource>(innerCall);
                }

                // If source is a constant wrapping our wrapper, get its items
                if (sourceExpr is ConstantExpression constExpr &&
                    constExpr.Value != null &&
                    constExpr.Value.GetType().IsGenericType &&
                    constExpr.Value.GetType().GetGenericTypeDefinition() == typeof(AsyncQueryableWrapper<>))
                {
                    var itemsProp = constExpr.Value.GetType().GetProperty("Items");
                    if (itemsProp != null)
                    {
                        var items = itemsProp.GetValue(constExpr.Value) as IEnumerable<object>;
                        return items?.Cast<object>().ToList();
                    }
                }
            }
        }

        // Return the current provider's items as fallback
        return _items.Cast<object>().ToList();
    }

    public object? Execute(Expression expression) => null;

    public TResult Execute<TResult>(Expression expression) => default!;
}

internal class AsyncEnumeratorWrapper<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _enumerator;

    public AsyncEnumeratorWrapper(IEnumerator<T> enumerator)
    {
        _enumerator = enumerator;
    }

    public T Current => _enumerator.Current;

    public ValueTask DisposeAsync()
    {
        _enumerator.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync() => new(_enumerator.MoveNext());
}

public class UserRoleServiceFixture
{
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IRepository<SecurityBootstrapClaim> _securityBootstrapClaimRep =
        Substitute.For<IRepository<SecurityBootstrapClaim>>();

    #region GetRolesByUserIdAsync

    [Fact]
    public async Task GetRolesByUserIdAsync_WithUserId_ReturnsRoles()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var userRoleId = Guid.NewGuid();

        var userRoles = new List<UserRole>
        {
            new UserRole
            {
                Id = userRoleId,
                UserId = userId,
                RoleId = roleId,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = Guid.NewGuid()
            }
        };

        var roles = new List<Role>
        {
            new Role
            {
                Id = roleId,
                Pid = Guid.Empty,
                Name = "TestRole",
                Code = "TEST_ROLE",
                Status = StatusEnum.Enable
            }
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleListAsync(Arg.Any<List<Guid>>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(roles);

        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        mockMapper.Map<List<UserRoleDto>>(Arg.Any<List<UserRole>>())
            .Returns(userRoles.Select(ur => new UserRoleDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                RoleId = ur.RoleId
            }).ToList());
        mockMapper.Map<List<RoleDto>>(Arg.Any<List<Role>>())
            .Returns(roles.Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code
            }).ToList());

        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();
        var asyncQueryable = new AsyncQueryableWrapper<UserRole>(userRoles);
        mockUserRoleRep.QueryNoTracking().Returns(asyncQueryable);

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var response = await service.GetRolesByUserIdAsync(new GetRolesByUserIdRequest { UserId = userId }, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.UserRoles.ShouldNotBeEmpty();
        response.Data.UserRoles.Count.ShouldBe(1);
        response.Data.Roles.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task GetRolesByUserIdAsync_WithNoUserRoles_ReturnsEmptyData()
    {
        var userId = Guid.NewGuid();
        var emptyUserRoles = new List<UserRole>();

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();

        var asyncQueryable = new AsyncQueryableWrapper<UserRole>(emptyUserRoles);
        mockUserRoleRep.QueryNoTracking().Returns(asyncQueryable);

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var response = await service.GetRolesByUserIdAsync(new GetRolesByUserIdRequest { UserId = userId }, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Data.ShouldNotBeNull();
        response.Data.UserRoles.ShouldBeEmpty();
        response.Data.Roles.ShouldBeEmpty();
    }

    #endregion

    #region AssignRolesToUserAsync

    [Fact]
    public async Task AssignRolesToUserAsync_WithNonExistentUser_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var command = new AssignRolesToUserCommand
        {
            UserId = userId,
            RoleIds = [roleId]
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        mockAccountDataProvider.GetUserAccountAsync(userId, string.Empty, Arg.Any<CancellationToken>())
            .Returns((UserAccountDto?)null);

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var response = await service.AssignRolesToUserAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.Message.ShouldContain("does not exist");
    }

    [Fact]
    public async Task AssignRolesToUserAsync_WithNonExistentRole_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var nonExistentRoleId = Guid.NewGuid();

        var command = new AssignRolesToUserCommand
        {
            UserId = userId,
            RoleIds = [nonExistentRoleId]
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleListAsync(Arg.Any<List<Guid>>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Role>());

        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        mockAccountDataProvider.GetUserAccountAsync(userId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto { Id = userId, Name = "Test" });

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var response = await service.AssignRolesToUserAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        response.Code.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        response.Message.ShouldContain("do not exist");
    }

    [Fact]
    public async Task AssignRolesToUserAsync_WithValidData_CreatesUserRoles()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var command = new AssignRolesToUserCommand
        {
            UserId = userId,
            RoleIds = [roleId]
        };

        var roles = new List<Role>
        {
            new Role { Id = roleId, Pid = Guid.Empty, Name = "TestRole", Code = "TEST", Status = StatusEnum.Enable }
        };

        // Empty existing user roles - this is what the Where/Select will return
        var existingUserRoles = new List<UserRole>();

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleListAsync(Arg.Any<List<Guid>>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(roles);

        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        mockAccountDataProvider.GetUserAccountAsync(userId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto { Id = userId, Name = "Test" });

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();

        // Set up the async queryable for the empty user roles
        var asyncQueryable = new AsyncQueryableWrapper<UserRole>(existingUserRoles);
        mockUserRoleRep.QueryNoTracking().Returns(asyncQueryable);
        mockUserRoleRep.InsertManyAsync(Arg.Any<List<UserRole>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var response = await service.AssignRolesToUserAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockUserRoleRep.Received().InsertManyAsync(
            Arg.Is<List<UserRole>>(urs => urs.Count == 1 && urs[0].UserId == userId && urs[0].RoleId == roleId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignRolesToUserAsync_WithAlreadyAssignedRole_NoOp()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var command = new AssignRolesToUserCommand
        {
            UserId = userId,
            RoleIds = [roleId]
        };

        var roles = new List<Role>
        {
            new Role { Id = roleId, Pid = Guid.Empty, Name = "TestRole", Code = "TEST", Status = StatusEnum.Enable }
        };

        // User already has this role assigned
        var existingUserRoles = new List<UserRole>
        {
            new UserRole { Id = Guid.NewGuid(), UserId = userId, RoleId = roleId }
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleListAsync(Arg.Any<List<Guid>>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(roles);

        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        mockAccountDataProvider.GetUserAccountAsync(userId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto { Id = userId, Name = "Test" });

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();

        var asyncQueryable = new AsyncQueryableWrapper<UserRole>(existingUserRoles);
        mockUserRoleRep.QueryNoTracking().Returns(asyncQueryable);

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var response = await service.AssignRolesToUserAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockUserRoleRep.DidNotReceive().InsertManyAsync(Arg.Any<List<UserRole>>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region UnassignRolesFromUserAsync

    [Fact]
    public async Task UnassignRolesFromUserAsync_WithValidData_DeletesUserRoles()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var command = new UnassignRolesFromUserCommand
        {
            UserId = userId,
            RoleIds = [roleId]
        };

        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();
        mockUserRoleRep.DeleteAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var response = await service.UnassignRolesFromUserAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockUserRoleRep.Received().DeleteAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region CheckUserIsSuperAdminAsync

    [Fact]
    public async Task CheckUserIsSuperAdminAsync_WithSuperAdminRole_ReturnsTrue()
    {
        var userId = Guid.NewGuid();

        var roles = new List<Role>
        {
            new Role
            {
                Id = RoleConstants.SuperAdminId,
                Pid = Guid.Empty,
                Name = RoleConstants.SuperAdmin,
                Code = RoleConstants.SuperAdmin,
                Status = StatusEnum.Enable
            }
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleListAsync(Arg.Any<List<Guid>>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(roles);

        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();
        mockUserRoleRep.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var result = await service.CheckUserIsSuperAdminAsync(userId, CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task CheckUserIsSuperAdminAsync_WithNoSuperAdminRole_ReturnsFalse()
    {
        var userId = Guid.NewGuid();

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleListAsync(Arg.Any<List<Guid>>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(new List<Role>());

        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();
        mockUserRoleRep.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var result = await service.CheckUserIsSuperAdminAsync(userId, CancellationToken.None);

        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CheckUserIsSuperAdminAsync_WithNoUserRole_ReturnsFalse()
    {
        var userId = Guid.NewGuid();
        var roles = new List<Role>
        {
            new Role
            {
                Id = RoleConstants.SuperAdminId,
                Name = RoleConstants.SuperAdmin,
                Code = RoleConstants.SuperAdmin,
                Status = StatusEnum.Enable
            }
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleListAsync(Arg.Any<List<Guid>>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(roles);

        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();
        mockUserRoleRep.AnyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        var mockMapper = Substitute.For<AutoMapper.IMapper>();

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var result = await service.CheckUserIsSuperAdminAsync(userId, CancellationToken.None);

        result.ShouldBeFalse();
    }

    #endregion

    #region ReplaceUserRolesAsync

    [Fact]
    public async Task ReplaceUserRolesAsync_WithValidData_ReplacesUserRoles()
    {
        var userId = Guid.NewGuid();
        var oldRoleId = Guid.NewGuid();
        var newRoleId = Guid.NewGuid();

        var command = new ReplaceUserRolesCommand
        {
            UserId = userId,
            RoleIds = [newRoleId]
        };

        var roles = new List<Role>
        {
            new Role { Id = newRoleId, Pid = Guid.Empty, Name = "NewRole", Code = "NEW", Status = StatusEnum.Enable }
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        mockRoleDataProvider.GetRoleListAsync(Arg.Any<List<Guid>>(), Arg.Any<List<string>>(), Arg.Any<CancellationToken>())
            .Returns(roles);

        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        mockAccountDataProvider.GetUserAccountAsync(userId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto { Id = userId, Name = "Test" });

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();
        mockUserRoleRep.QueryNoTracking().Returns(new AsyncQueryableWrapper<UserRole>(
        [
            new UserRole { UserId = userId, RoleId = oldRoleId }
        ]));
        mockUserRoleRep.DeleteAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        mockUserRoleRep.InsertManyAsync(Arg.Any<List<UserRole>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var response = await service.ReplaceUserRolesAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockUserRoleRep.Received().DeleteAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), Arg.Any<CancellationToken>());
        await mockUserRoleRep.Received().InsertManyAsync(
            Arg.Is<List<UserRole>>(urs => urs.Count == 1 && urs[0].RoleId == newRoleId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReplaceUserRolesAsync_WithEmptyRoles_DeletesAllUserRoles()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var command = new ReplaceUserRolesCommand
        {
            UserId = userId,
            RoleIds = []
        };

        var mockRoleDataProvider = Substitute.For<IRoleDataProvider>();
        var mockAccountDataProvider = Substitute.For<IAccountDataProvider>();
        mockAccountDataProvider.GetUserAccountAsync(userId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto { Id = userId, Name = "Test" });

        var mockMapper = Substitute.For<AutoMapper.IMapper>();
        var mockUserRoleRep = Substitute.For<IRepository<UserRole>>();
        mockUserRoleRep.QueryNoTracking().Returns(new AsyncQueryableWrapper<UserRole>(
        [
            new UserRole { UserId = userId, RoleId = roleId }
        ]));
        mockUserRoleRep.DeleteAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var service = new UserRoleService(
            mockMapper,
            mockRoleDataProvider,
            mockAccountDataProvider,
            mockUserRoleRep,
            _securityBootstrapClaimRep,
            _currentUser);

        var response = await service.ReplaceUserRolesAsync(command, CancellationToken.None);

        response.ShouldNotBeNull();
        await mockUserRoleRep.Received().DeleteAsync(Arg.Any<System.Linq.Expressions.Expression<Func<UserRole, bool>>>(), Arg.Any<CancellationToken>());
        await mockUserRoleRep.DidNotReceive().InsertManyAsync(Arg.Any<List<UserRole>>(), Arg.Any<CancellationToken>());
    }

    #endregion
}