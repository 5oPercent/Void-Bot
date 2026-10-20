using System.Net;

namespace Void.Bot.Core.Exceptions;

/// <summary>写入已开始后发现并发冲突，必须抛出异常使外层工作单元回滚整次操作。</summary>
public sealed class EraEquipmentWriteConflictException : BusinessException
{
    public EraEquipmentWriteConflictException(string message)
        : base(HttpStatusCode.Conflict, message, "era.equipment_version_conflict")
    {
    }
}
