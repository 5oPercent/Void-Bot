using System.Net;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Services.ERA.Configuration;

public sealed partial class EraAbilityCatalogQueryService
{
    public async Task<GetEraDataReferencesResponse> GetDataReferencesAsync(GetEraDataReferencesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            EraDataDefinitionValidator.Id(request.Id);
            ValidatePaging(request.Skip, request.Take);
            if (!Enum.IsDefined(request.Kind))
            {
                return DataInvalid<GetEraDataReferencesResponse>();
            }
        }
        catch (ArgumentException)
        {
            return DataInvalid<GetEraDataReferencesResponse>();
        }

        var view = await ReadAsync(cancellationToken).ConfigureAwait(false);
        if (view == null)
        {
            return DataUnavailable<GetEraDataReferencesResponse>();
        }

        var configuration = EraDataCatalog.Compile(view);
        var references = new List<EraDataReference>();
        void Scan(IEnumerable<string> ids, string type, string id, string path)
        {
            if (ids.Contains(request.Id, StringComparer.Ordinal))
            {
                references.Add(new() { Type = type, Id = id, Path = path });
            }
        }

        if (request.Kind == EraDataKind.Resonance)
        {
            foreach (var system in configuration.Systems)
            {
                Scan(system.SpecialSkills, "System", system.Id, "Systems/" + system.Id + "/SpecialSkills");
                Scan(system.PrimarySkills, "System", system.Id, "Systems/" + system.Id + "/PrimarySkills");
                Scan(system.SecondarySkills, "System", system.Id, "Systems/" + system.Id + "/SecondarySkills");
                Scan(system.TacticalSkills, "System", system.Id, "Systems/" + system.Id + "/TacticalSkills");
            }

        }

        if (request.Kind == EraDataKind.Buff)
        {
            foreach (var skill in configuration.Skills)
            {
                foreach (var variant in skill.Variants)
                {
                    Scan(variant.Effects.Select(effect => effect.EffectId), "Resonance", skill.Id, "Skills/" + skill.Id + "/Variants/" + variant.Slot + "/Effects");
                }
            }
        }

        foreach (var enemy in configuration.Enemies ?? new())
        {
            if (request.Kind == EraDataKind.Resonance) Scan(enemy.Skills.Select(s => s.SkillId), "Enemy", enemy.Id, "Enemies/" + enemy.Id + "/Skills");
            if (request.Kind == EraDataKind.Buff) Scan(enemy.InitialBuffs, "Enemy", enemy.Id, "Enemies/" + enemy.Id + "/InitialBuffs");
        }

        return new()
        {
            Code = HttpStatusCode.OK,
            Data = new()
            {
                Revision = view.Workspace.Revision,
                Total = references.Count,
                Items = references.Skip(request.Skip).Take(request.Take).ToList()
            }
        };
    }

}
