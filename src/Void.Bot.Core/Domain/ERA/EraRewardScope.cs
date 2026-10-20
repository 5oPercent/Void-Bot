using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Domain.ERA;

/// <summary>串行化同作用域的请求登记、业务事件防重及内部发放；不替代世界权限协调行。</summary>
[Table("era_reward_scope")]
public sealed class EraRewardScope : IEntityBase
{
    [Key]
    public Guid ScopeId { get; set; }
}
