using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.Email;

namespace Void.Bot.Core.Domain.Email;

[Table("email_credential")]
public class EmailCredential : IEntity<Guid>, IEntityCreated, IEntityModified
{
    public static readonly Guid DefaultId = Guid.Parse("e1000000-0000-0000-0000-000000000001");

    [Key]
    [Column("id")]
    public Guid Id { get; set; } = DefaultId;

    [Column("sender_email")]
    [StringLength(320)]
    public string SenderEmail { get; set; } = string.Empty;

    [Column("external_account_id")]
    [StringLength(255)]
    public string ExternalAccountId { get; set; } = string.Empty;

    [Column("encrypted_access_token")]
    public string EncryptedAccessToken { get; set; } = string.Empty;

    [Column("access_token_expires_at")]
    public DateTimeOffset AccessTokenExpiresAt { get; set; }

    [Column("encrypted_refresh_token")]
    public string EncryptedRefreshToken { get; set; } = string.Empty;

    [Column("refresh_token_last_used_at")]
    public DateTimeOffset RefreshTokenLastUsedAt { get; set; }

    [Column("next_refresh_at")]
    public DateTimeOffset NextRefreshAt { get; set; }

    [Column("status")]
    public EmailCredentialStatusEnum Status { get; set; }

    [Column("last_error")]
    [StringLength(512)]
    public string? LastError { get; set; }

    [Column("last_error_at")]
    public DateTimeOffset? LastErrorAt { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("update_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
