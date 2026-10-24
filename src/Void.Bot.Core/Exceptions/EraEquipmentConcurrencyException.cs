namespace Void.Bot.Core.Exceptions;

/// <summary>装备读取或条件写入发生并发变化；不向业务层暴露持久化框架异常。</summary>
public sealed class EraEquipmentConcurrencyException : Exception
{
    public EraEquipmentConcurrencyException(string message) : base(message)
    {
    }
}
