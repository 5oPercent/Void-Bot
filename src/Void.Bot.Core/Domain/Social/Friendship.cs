using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Enum.Social;

namespace Void.Bot.Core.Domain.Social;

[Table("friendship")]
[Index(nameof(FirstAccountId), nameof(SecondAccountId), IsUnique = true)]
[Index(nameof(SecondAccountId))]
public class Friendship : IEntity<Guid>, IEntityCreated
{
    [Key]
    public Guid Id { get; set; }
    public Guid FirstAccountId { get; set; }
    public Guid SecondAccountId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
