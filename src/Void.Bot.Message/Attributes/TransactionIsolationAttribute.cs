using System.Data;

namespace Void.Bot.Message.Attributes;

/// <summary>声明消息所需的事务隔离级别；未声明时沿用数据库默认级别。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class TransactionIsolationAttribute : Attribute
{
    public IsolationLevel IsolationLevel { get; }

    public TransactionIsolationAttribute(IsolationLevel isolationLevel)
    {
        IsolationLevel = isolationLevel;
    }
}
