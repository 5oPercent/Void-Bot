namespace Void.Bot.Core.Domain;

public interface IHasModifiedFields
{
    Guid? LastModifiedBy { get; set; }
    
    DateTimeOffset LastModifiedDate { get; set; }
}