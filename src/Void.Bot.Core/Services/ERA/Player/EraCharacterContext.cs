using System.Net;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.Language;

namespace Void.Bot.Core.Services.ERA.Player;

public interface IEraCharacterContext : IScope
{
    Task<EraCharacter> RequireOwnedAsync(Guid characterId, bool forUpdate, CancellationToken cancellationToken);
    Task<EraCharacter> RequireTargetAsync(Guid characterId, bool forUpdate, CancellationToken cancellationToken);
}

/// <summary>角色归属校验；写入统一先锁玩家再锁角色，与角色禁用的锁顺序一致。</summary>
public sealed class EraCharacterContext : IEraCharacterContext
{
    private readonly IEraDataProvider _characters;
    private readonly IEraPlayerContext _players;
    private readonly ILocalizationService _localization;

    public EraCharacterContext(IEraDataProvider characters, IEraPlayerContext players, ILocalizationService localization)
    {
        _characters = characters;
        _players = players;
        _localization = localization;
    }

    public async Task<EraCharacter> RequireOwnedAsync(Guid characterId, bool forUpdate, CancellationToken cancellationToken)
    {
        var player = await _players.RequireCurrentAsync(forUpdate, cancellationToken).ConfigureAwait(false);
        var character = await ReadCharacterAsync(characterId, forUpdate, cancellationToken).ConfigureAwait(false);
        if (character == null || character.PlayerId != player.Id)
        {
            throw CharacterNotFound();
        }

        return character;
    }

    public async Task<EraCharacter> RequireTargetAsync(Guid characterId, bool forUpdate, CancellationToken cancellationToken)
    {
        var initial = await ReadCharacterAsync(characterId, false, cancellationToken).ConfigureAwait(false);
        if (initial == null)
        {
            throw CharacterNotFound();
        }

        await _players.RequireTargetAsync(initial.PlayerId, forUpdate, cancellationToken).ConfigureAwait(false);
        var character = forUpdate
            ? await ReadCharacterAsync(characterId, true, cancellationToken).ConfigureAwait(false)
            : initial;
        if (character == null || character.PlayerId != initial.PlayerId)
        {
            throw CharacterNotFound();
        }

        return character;
    }

    private async Task<EraCharacter?> ReadCharacterAsync(Guid characterId, bool forUpdate, CancellationToken cancellationToken)
    {
        if (characterId == Guid.Empty)
        {
            throw CharacterNotFound();
        }

        if (forUpdate)
        {
            return await _characters.LockCharacterAsync(characterId, cancellationToken).ConfigureAwait(false);
        }

        return await _characters.GetCharacterByIdAsync(characterId, cancellationToken).ConfigureAwait(false);
    }

    private EraPlayerAccessException CharacterNotFound()
    {
        return new EraPlayerAccessException(HttpStatusCode.NotFound, "era.character_not_found",
            _localization.GetValue<EraContextTemplate>(text => text.CharacterNotFound));
    }
}
