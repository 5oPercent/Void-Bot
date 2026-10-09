namespace Void.Bot.Message.Dto.ERA;

/// <summary>永久玩家记录，不保存最终属性、运行时血量或 BUFF。</summary>
public class EraPlayerDto
{
    /// <summary>ERA 玩家档案 ID，也是共享仓库的归属 ID。</summary>
    public Guid Id { get; set; }

    public int WarehouseCapacity { get; set; }
    public long Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>玩家档案下的角色配置；仓库不存放在角色记录中。</summary>
public class EraCharacterDto
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string CharacterDefinitionId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    /// <summary>角色是否已禁用；客户端根据此状态决定显示与使用方式。</summary>
    public bool IsDisabled { get; set; }

    public long Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class EraCapacityReceiptDto
{
    public Guid RequestId { get; set; }
    public Guid PlayerId { get; set; }
    public int PreviousCapacity { get; set; }
    public int WarehouseCapacity { get; set; }
    public long PlayerVersion { get; set; }
}

/// <summary>稳定业务错误码；客户端根据此字段处理，不解析本地化文案。</summary>
public interface IEraErrorResponse
{
    string? ErrorCode { get; set; }
}

public class EraResponse<T> : VoidBotResponse<T>, IEraErrorResponse
{
}
