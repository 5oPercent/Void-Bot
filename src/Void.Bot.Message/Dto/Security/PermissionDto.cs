using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Security;

namespace Void.Bot.Message.Dto.Security;

public abstract class PermissionBaseDto
{
    public Guid Pid { get; set; }
    
    public PermissionTypeEnum Type { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
    
    public int Sequence { get; set; }

    public StatusEnum Status { get; set; }
}

public class PermissionDto : PermissionBaseDto, ITreeEntityAudit<Guid, PermissionDto>
{
    public Guid Id { get; set; }
    
    [NotMapped]
    public List<PermissionDto> Children { get; set; } = [];
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public Guid? LastModifiedBy { get; set; }
}