namespace Void.Bot.Core.Domain;

public interface IHasCreatedFields
{
    Guid? CreatedBy { get; set; }
    
    DateTimeOffset CreatedDate { get; set; }
}