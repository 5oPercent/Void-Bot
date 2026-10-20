using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>队伍 Redis 提交与 SQL 世界写入共用的持久锁行；不复制 Redis 队伍数据。</summary>
[Table("era_world_team_state")]
public sealed class EraWorldTeamState : IEntityBase
{
    [Key]
    public Guid TeamId { get; set; }
    public Guid? ActiveWorldId { get; set; }
}
