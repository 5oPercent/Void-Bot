using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Models;

public sealed record EraWorldRewardContext(EraWorld World, EraTeam Team, IReadOnlyList<EraWorldMember> Members);
