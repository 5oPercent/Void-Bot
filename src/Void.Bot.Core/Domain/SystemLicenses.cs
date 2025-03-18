using System.ComponentModel.DataAnnotations.Schema;

namespace Void.Bot.Core.Domain;

[Table("system_licenses")]
public class SystemLicenses : IEntityAudit
{
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("api_key")]
    public string ApiKey { get; set; }
    
    [Column("user_name")]
    public string UserName { get; set; }
    
    public Guid CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    public Guid? LastModifiedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}