namespace Void.Bot.Core.Domain;

public interface IEntityBase { }

public interface IEntity<T> : IEntityBase
{
    T Id { get; set; }
}

public interface IEntityAudit<T> : IEntity<T>, IEntityCreated, IEntityModified
{
    public Guid CreatedBy { get; set; }

    public Guid? LastModifiedBy { get; set; }
}

public interface IEntityCreated : IEntityBase
{
    public DateTimeOffset CreatedAt { get; set; }
}

public interface IEntityModified : IEntityBase
{
    public DateTimeOffset? UpdatedAt { get; set; }
}