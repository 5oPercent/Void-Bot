using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Security;

namespace Void.Bot.Core.Domain.Security;

[Table("Menu")]
public class Menu : IEntityAudit<Guid>
{
    public Guid Id { get; set; }
    
    public Guid Pid { get; set; }

    [NotMapped]
    public List<Menu> Children { get; set; } = [];

    public MenuTypeEnum Type { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;
    
    public string Icon { get; set; } = string.Empty;
    
    public int Sequence { get; set; }

    public StatusEnum Status { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Guid CreatedBy { get; set; }
    
    public Guid? LastModifiedBy { get; set; }
}