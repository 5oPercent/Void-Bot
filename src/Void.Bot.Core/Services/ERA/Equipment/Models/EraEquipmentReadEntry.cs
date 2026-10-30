using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment.Models;

public sealed record EraEquipmentReadEntry(EraEquipmentInstance Equipment, EraArmorInstance? Armor,
    Void.Bot.Message.Dto.ERA.EraWeaponInstanceDto? Weapon = null,
    Void.Bot.Message.Dto.ERA.EraEquipmentShareDto? Share = null);
