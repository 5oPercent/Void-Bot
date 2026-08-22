using Void.Bot.Core.Services;

namespace Void.Bot.Core.Middlewares.UnitofWork;

public interface IUnitOfWorkExecutionContext : IScope
{
    UnitOfWorkExecutionFrame Enter(object context, bool transactionExists);

    bool TryExit(object context, out UnitOfWorkExecutionFrame frame);

    /// <summary>
    /// 登记仅在最外层业务事务提交成功后执行的回调.
    /// </summary>
    void EnqueueAfterCommit(Func<CancellationToken, Task> callback);

    IReadOnlyList<Func<CancellationToken, Task>> TakeAfterCommitCallbacks();

    void ClearAfterCommitCallbacks();
}

public readonly record struct UnitOfWorkExecutionFrame(bool IsOutermost, bool OwnsTransaction);

public class UnitOfWorkExecutionContext : IUnitOfWorkExecutionContext
{
    private readonly Dictionary<object, UnitOfWorkExecutionFrame> _frames =
        new(ReferenceEqualityComparer.Instance);
    private readonly List<Func<CancellationToken, Task>> _afterCommitCallbacks = [];
    private int _depth;

    public UnitOfWorkExecutionFrame Enter(object context, bool transactionExists)
    {
        var frame = new UnitOfWorkExecutionFrame(
            IsOutermost: _depth == 0,
            OwnsTransaction: _depth == 0 && !transactionExists);
        _frames.Add(context, frame);
        _depth++;
        return frame;
    }

    public bool TryExit(object context, out UnitOfWorkExecutionFrame frame)
    {
        if (!_frames.Remove(context, out frame))
        {
            return false;
        }

        _depth--;
        return true;
    }

    public void EnqueueAfterCommit(Func<CancellationToken, Task> callback)
    {
        _afterCommitCallbacks.Add(callback);
    }

    public IReadOnlyList<Func<CancellationToken, Task>> TakeAfterCommitCallbacks()
    {
        var callbacks = _afterCommitCallbacks.ToList();
        _afterCommitCallbacks.Clear();
        return callbacks;
    }

    public void ClearAfterCommitCallbacks()
    {
        _afterCommitCallbacks.Clear();
    }
}
