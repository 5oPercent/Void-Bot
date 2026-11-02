using System.Text.Json;
using Shouldly;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.UnitTest.ERA;

public sealed class LatticeResonanceTerminologyFixture
{
    [Fact]
    public void Renamed_definitions_read_existing_configuration_and_keep_its_transport_fields()
    {
        const string legacy = """
            {"SchemaVersion":3,"Version":7,"Systems":[{"Id":"old.system","Name":"Existing name","PrimarySkills":["old.skill"]}],"Skills":[{"Id":"old.skill","Variants":[{"Slot":1,"MaximumCharges":3}]}]}
            """;
        var configuration = JsonSerializer.Deserialize<ResonanceConfiguration>(legacy)!;
        configuration.Systems.Single().ShouldBeOfType<LatticeDefinition>();
        configuration.Skills.Single().ShouldBeOfType<ResonanceDefinition>();
        configuration.Skills.Single().Variants.Single().ShouldBeOfType<ResonanceVariant>();
        configuration.Systems.Single().PrimarySkills.Single().ShouldBe("old.skill");
        configuration.Skills.Single().Variants.Single().MaximumCharges.ShouldBe(3);

        using var stored = JsonDocument.Parse(JsonSerializer.Serialize(configuration));
        stored.RootElement.GetProperty("Systems")[0].GetProperty("PrimarySkills")[0].GetString().ShouldBe("old.skill");
        stored.RootElement.GetProperty("Skills")[0].GetProperty("Variants")[0].GetProperty("Slot").GetInt32().ShouldBe(1);
        stored.RootElement.TryGetProperty("Lattices", out _).ShouldBeFalse();
        stored.RootElement.TryGetProperty("Resonances", out _).ShouldBeFalse();

        using var http = JsonDocument.Parse(JsonSerializer.Serialize(configuration, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        http.RootElement.GetProperty("systems")[0].GetProperty("id").GetString().ShouldBe("old.system");
        http.RootElement.GetProperty("skills")[0].GetProperty("id").GetString().ShouldBe("old.skill");
    }
}
