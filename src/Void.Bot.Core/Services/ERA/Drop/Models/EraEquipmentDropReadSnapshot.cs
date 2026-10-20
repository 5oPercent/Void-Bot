using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Drop.Models;

public sealed record EraEquipmentDropReadSnapshot(EraWorld World, EraWorldMember Member, EraCharacter Character,
    Guid? ActiveWorldId, IReadOnlyList<EraEquipmentDropReadEntry> Entries);
