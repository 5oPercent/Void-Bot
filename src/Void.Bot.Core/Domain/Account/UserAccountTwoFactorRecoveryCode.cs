using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Account;

[Table("user_account_two_factor_recovery_code")]
public sealed class UserAccountTwoFactorRecoveryCode : IEntity<Guid>, IEntityCreated
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(64)]
    [Column("code_hash")]
    public string CodeHash { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
