using System.ComponentModel.DataAnnotations.Schema;

namespace Void.Bot.Core.Domain.Security;

[Table("User_Role")]
public class UserRole : IEntityAudit<Guid>
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }
    
    public DateTimeOffset Deadline { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public Guid? LastModifiedBy { get; set; }
}