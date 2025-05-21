using System.ComponentModel.DataAnnotations.Schema;

namespace Void.Bot.Core.Domain.Security;

[Table("Role_Permission")]
public class RolePermission : IEntityAudit<Guid>
{
    public Guid Id { get; set; }
    
    public Guid RoleId { get; set; }
    
    public Guid PermissionId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public Guid? LastModifiedBy { get; set; }
}