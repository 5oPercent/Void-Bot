namespace Void.Bot.Core.Domain;

public interface IEntityBase { }

public interface IEntity : IEntityBase
{
    public Guid Id { get; set; }
}

public interface IEntity<T> : IEntityBase
{
    T Id { get; set; }
}