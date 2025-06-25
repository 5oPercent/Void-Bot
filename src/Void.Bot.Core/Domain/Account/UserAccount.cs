using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Account;

[Table("user_account")]
public class UserAccount : IEntity<Guid>, IEntityCreated, IEntityModified
{
    [Key] [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Column("name")]
    [StringLength(64)]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("Suffix")] [StringLength(4)]
    public string Suffix { get; set; } = "".GenerateNumberSuffix();
    
    [NotMapped]
    public string DisplayName => $"{Name}#{Suffix}";

    [Column("password")]
    [StringLength(255)]
    [Required]
    public string Password { get; set; } = string.Empty;
    
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("update_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
    
    [NotMapped]
    public List<UserThirdParty> ThirdParties { get; set; } = new();
}