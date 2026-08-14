using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Security;

[Table("security_bootstrap_claim")]
public class SecurityBootstrapClaim : IEntity<string>, IEntityCreated
{
    [Key]
    [Column("claim_key")]
    public string Id { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}