using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public partial interface IEraEquipmentDataProvider : IScope
{
    Task<EraEquipmentConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken);
    Task<EraEquipmentReadSnapshot?> GetCharacterSnapshotAsync(
        Guid characterId, Guid playerId, CancellationToken cancellationToken);
}

/// <summary>装备查询与位置变更的数据访问；Request 不产生写入，事务由工作单元提交。</summary>
public sealed partial class EraEquipmentDataProvider : IEraEquipmentDataProvider
{
    private static readonly Guid ConfigurationId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private readonly VoidBotDbContext _dbContext;

    public EraEquipmentDataProvider(VoidBotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EraEquipmentConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Set<EraEquipmentConfiguration>().AsNoTracking()
            .SingleOrDefaultAsync(configuration => configuration.Id == ConfigurationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<EraEquipmentReadSnapshot?> GetCharacterSnapshotAsync(
        Guid characterId, Guid playerId, CancellationToken cancellationToken)
    {
        // 一条 SQL 同时读取状态版本、两种位置与保存属性；LEFT JOIN 保留真正的空背包。
        var rows = await (
            from character in _dbContext.Set<EraCharacter>()
            where character.Id == characterId && character.PlayerId == playerId
            join equipment in _dbContext.Set<EraEquipmentInstance>().Where(equipment =>
                equipment.Location == EraEquipmentLocation.Inventory || equipment.Location == EraEquipmentLocation.Equipped)
                on (Guid?)character.Id equals equipment.CharacterId into equipmentGroup
            from equipment in equipmentGroup.DefaultIfEmpty()
            join armor in _dbContext.Set<EraArmorInstanceData>()
                on equipment!.Id equals armor.EquipmentId into armorGroup
            from armor in armorGroup.DefaultIfEmpty()
            join member in _dbContext.Set<EraWorldMember>().Where(member => member.IsContinuing)
                on (Guid?)character.Id equals member.ActiveCharacterId into memberGroup
            from member in memberGroup.DefaultIfEmpty()
            join world in _dbContext.Set<EraWorld>().Where(world => world.State == EraWorldState.Building ||
                    world.State == EraWorldState.Ready || world.State == EraWorldState.Ending)
                on member!.WorldId equals world.Id into worldGroup
            from world in worldGroup.DefaultIfEmpty()
            select new
            {
                character.Id,
                character.PlayerId,
                character.IsDisabled,
                character.EquipmentVersion,
                Equipment = equipment,
                Armor = armor,
                World = world
            }).AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);

        if (rows.Count == 0)
        {
            return null;
        }

        var first = rows[0];
        return new EraEquipmentReadSnapshot
        {
            CharacterId = first.Id,
            PlayerId = first.PlayerId,
            IsCharacterDisabled = first.IsDisabled,
            EquipmentVersion = first.EquipmentVersion,
            EquippedOperationRestriction = first.World == null ? null : new EraEquippedOperationRestrictionDto
            {
                WorldInstanceId = first.World.Id, WorldVersion = first.World.Version,
                RestrictionVersion = first.World.RestrictionVersion, IsRestricted = first.World.IsEquippedOperationsRestricted
            },
            Entries = rows.Where(row => row.Equipment != null)
                .Select(row => new EraEquipmentReadEntry(row.Equipment!, row.Armor)).ToList()
        };
    }
}
