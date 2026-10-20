using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Models;

public sealed record EraEquipmentDropReadEntry(EraEquipmentDrop Drop, EraEquipmentInstance? Equipment, EraArmorInstanceData? Armor);
