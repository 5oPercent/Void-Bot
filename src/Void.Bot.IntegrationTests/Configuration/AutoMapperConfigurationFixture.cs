using AutoMapper;
using AutoMapper.Internal;
using Shouldly;

namespace Void.Bot.IntegrationTest.Configuration;

public class AutoMapperConfigurationFixture(IntegrationTestBase testBase) : IntegrationFixture(testBase)
{
    [Fact]
    public async Task AllTypeMapsShouldHaveMaxDepthLimitAsync()
    {
        await Run<IConfigurationProvider>(mapperConfiguration =>
        {
            var typeMaps = mapperConfiguration.Internal().GetAllTypeMaps();

            typeMaps.ShouldNotBeEmpty();
            foreach (var typeMap in typeMaps)
            {
                typeMap.MaxDepth.ShouldBeGreaterThan(
                    0,
                    $"{typeMap.SourceType.Name} -> {typeMap.DestinationType.Name} should have a max depth limit");
            }

            return Task.CompletedTask;
        });
    }
}
