using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Models;

public sealed record EraEquipmentReadEntry(EraEquipmentInstance Equipment, EraArmorInstanceData? Armor);
