using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment.Models;

public sealed record EraEquipmentReadEntry(EraEquipmentInstance Equipment, EraArmorInstanceData? Armor);
