using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA;

public interface IEraEquipmentDataProvider : IScope
{
    Task<EraEquipmentConfiguration?> GetConfigurationAsync(CancellationToken cancellationToken);
    Task<EraEquipmentReadSnapshot?> GetCharacterSnapshotAsync(
        Guid characterId, Guid playerId, CancellationToken cancellationToken);
}

/// <summary>只读数据访问，不建表、初始化、加写锁或提交工作单元。</summary>
public sealed class EraEquipmentDataProvider : IEraEquipmentDataProvider
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
            select new
            {
                character.Id,
                character.PlayerId,
                character.IsDisabled,
                character.EquipmentVersion,
                Equipment = equipment,
                Armor = armor
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
            Entries = rows.Where(row => row.Equipment != null)
                .Select(row => new EraEquipmentReadEntry(row.Equipment!, row.Armor)).ToList()
        };
    }
}

/// <summary>单条查询产生的内部快照，不直接暴露持久化实体给 API。</summary>
public sealed class EraEquipmentReadSnapshot
{
    public Guid CharacterId { get; set; }
    public Guid PlayerId { get; set; }
    public bool IsCharacterDisabled { get; set; }
    public long EquipmentVersion { get; set; }
    public List<EraEquipmentReadEntry> Entries { get; set; } = new List<EraEquipmentReadEntry>();
}

public sealed record EraEquipmentReadEntry(EraEquipmentInstance Equipment, EraArmorInstanceData? Armor);
