using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Domain.Security;

[Table("Role")]
public class Role : ITreeEntityAudit<Guid, Role>
{
    public Guid Id { get; set; }

    public Guid Pid { get; set; }

    [NotMapped]
    public List<Role> Children { get; set; } = [];
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
    
    public int Sequence { get; set; }

    public StatusEnum Status { get; set; }

    public bool Built { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public Guid? LastModifiedBy { get; set; }
}