using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.World.Models;

public sealed record EraWorldRewardContext(EraWorld World, EraTeam Team, IReadOnlyList<EraWorldMember> Members);
