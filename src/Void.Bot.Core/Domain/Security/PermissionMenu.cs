using System.ComponentModel.DataAnnotations.Schema;

namespace Void.Bot.Core.Domain.Security;

[Table("Permission_Menu")]
public class PermissionMenu : IEntityAudit<Guid>
{
    public Guid Id { get; set; }

    public Guid PermissionId { get; set; }

    public Guid MenuId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public Guid? LastModifiedBy { get; set; }
}