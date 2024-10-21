using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Core.Extension;

namespace Void.Bot.Core.Domain.Account;

[Table("user_account")]
public class UserAccount : IEntity, IEntityCreated, IEntityModified
{
    [Key] [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Column("name")]
    [StringLength(64)]
    public string Name { get; set; }

    [Column("Suffix")] [StringLength(4)]
    public string Suffix { get; set; } = "".GenerateNumberSuffix();
    
    [NotMapped]
    public string DisplayName => $"{Name}#{Suffix}";
    
    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; }
    
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    [Column("update_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.Now;
    
    [NotMapped]
    public List<UserThirdParty> ThirdParties { get; set; } = new();
}