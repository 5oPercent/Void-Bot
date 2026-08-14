using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Security;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Security;

namespace Void.Bot.IntegrationTest.Utils;

public class SecurityUtil(IntegrationTestBase testBase) : IntegrationUtilBase(testBase)
{
    private static readonly Guid TestUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    public async Task<Guid> AddRoleAsync(string name = "", string code = "", Guid pid = default, bool built = false)
    {
        if (name.IsNullOrEmpty())
            name = "".GenerateRandomString();
        if (code.IsNullOrEmpty())
            code = "".GenerateRandomString();

        var id = Guid.NewGuid();

        await RunWithUnitOfWork<IRepository<Role>>(async rep =>
        {
            await rep.InsertAsync(new Role
            {
                Id = id,
                Pid = pid,
                Name = name,
                Code = code,
                Description = "Test role",
                Sequence = 1,
                Status = StatusEnum.Enable,
                Built = built,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = TestUserId
            });
        });

        return id;
    }

    public async Task<Guid> AddPermissionAsync(string name = "", string code = "", Guid pid = default, PermissionTypeEnum type = PermissionTypeEnum.Action)
    {
        if (name.IsNullOrEmpty())
            name = "".GenerateRandomString();
        if (code.IsNullOrEmpty())
            code = "".GenerateRandomString();

        var id = Guid.NewGuid();

        await RunWithUnitOfWork<IRepository<Permission>>(async rep =>
        {
            await rep.InsertAsync(new Permission
            {
                Id = id,
                Pid = pid,
                Name = name,
                Code = code,
                Description = "Test permission",
                Type = type,
                Sequence = 1,
                Status = StatusEnum.Enable,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = TestUserId
            });
        });

        return id;
    }

    public async Task<Guid> AddUserRoleAsync(Guid userId, Guid roleId, DateTimeOffset? deadline = null)
    {
        var id = Guid.NewGuid();

        await RunWithUnitOfWork<IRepository<UserRole>>(async rep =>
        {
            await rep.InsertAsync(new UserRole
            {
                Id = id,
                UserId = userId,
                RoleId = roleId,
                Deadline = deadline,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = TestUserId
            });
        });

        return id;
    }

    public async Task<Guid> AddRolePermissionAsync(Guid roleId, Guid permissionId)
    {
        var id = Guid.NewGuid();

        await RunWithUnitOfWork<IRepository<RolePermission>>(async rep =>
        {
            await rep.InsertAsync(new RolePermission
            {
                Id = id,
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = TestUserId
            });
        });

        return id;
    }

    public async Task<Guid> AddMenuAsync(
        string name = "", string path = "", StatusEnum status = StatusEnum.Enable)
    {
        if (name.IsNullOrEmpty())
            name = "".GenerateRandomString();
        if (path.IsNullOrEmpty())
            path = "/" + "".GenerateRandomString();

        var id = Guid.NewGuid();

        await RunWithUnitOfWork<IRepository<Menu>>(async rep =>
        {
            await rep.InsertAsync(new Menu
            {
                Id = id,
                Name = name,
                Path = path,
                Description = "Test menu",
                Type = MenuTypeEnum.Menu,
                Sequence = 1,
                Status = status,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = TestUserId
            });
        });

        return id;
    }

    public async Task<Guid> AddPermissionMenuAsync(Guid permissionId, Guid menuId)
    {
        var id = Guid.NewGuid();

        await RunWithUnitOfWork<IRepository<PermissionMenu>>(async rep =>
        {
            await rep.InsertAsync(new PermissionMenu
            {
                Id = id,
                PermissionId = permissionId,
                MenuId = menuId,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = TestUserId
            });
        });

        return id;
    }

    public async Task<Guid> AddPermissionEndpointAsync(Guid permissionId, string endpoint)
    {
        var id = Guid.NewGuid();

        await RunWithUnitOfWork<IRepository<PermissionEndpoint>>(async rep =>
        {
            await rep.InsertAsync(new PermissionEndpoint
            {
                Id = id,
                PermissionId = permissionId,
                Endpoint = endpoint,
                CreatedAt = DateTimeOffset.Now,
                CreatedBy = TestUserId
            });
        });

        return id;
    }
}