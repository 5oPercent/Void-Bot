using Void.Bot.Message.Enum.Security;

namespace Void.Bot.Message.Dto.Security;

public class AuthorizedRoleDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public DateTimeOffset? Deadline { get; set; }
}

public class AuthorizedPermissionDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public DateTimeOffset? Deadline { get; set; }
}

public class AuthorizedMenuDto
{
    public Guid Id { get; set; }

    public Guid Pid { get; set; }

    public MenuTypeEnum Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public int Sequence { get; set; }

    public DateTimeOffset? Deadline { get; set; }
}