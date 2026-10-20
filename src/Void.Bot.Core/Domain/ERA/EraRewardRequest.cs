using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

[Table("era_reward_request")]
[PrimaryKey(nameof(ScopeId), nameof(RequestId))]
public sealed class EraRewardRequest : IEntityBase
{
    public Guid ScopeId { get; set; }
    public Guid RequestId { get; set; }
    public Guid EventId { get; set; }
}
