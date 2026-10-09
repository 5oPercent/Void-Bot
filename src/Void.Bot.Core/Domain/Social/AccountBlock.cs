using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.Social;

[Table("account_block")]
[Index(nameof(AccountId), nameof(BlockedAccountId), IsUnique = true)]
public class AccountBlock : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid BlockedAccountId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
