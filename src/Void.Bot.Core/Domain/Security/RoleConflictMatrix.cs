using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Security;

[Table("role_conflict_matrix")]
public class RoleConflictMatrix : IEntityAudit<Guid>
{
    public Guid Id { get; set; }
    
    public Guid RoleAId { get; set; }

    public Guid RoleBId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public Guid? LastModifiedBy { get; set; }
}