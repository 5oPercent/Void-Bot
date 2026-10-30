using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Drop.Models;

public sealed record EraEquipmentDropReadEntry(EraEquipmentDrop Drop, EraEquipmentInstance? Equipment, EraArmorInstance? Armor,
    EraEquipmentShare? Share = null, Void.Bot.Message.Dto.ERA.EraWeaponInstanceDto? Weapon = null);
