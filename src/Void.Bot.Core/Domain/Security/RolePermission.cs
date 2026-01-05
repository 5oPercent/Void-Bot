using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Security;

[Table("role_permission")]
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