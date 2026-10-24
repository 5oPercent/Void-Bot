using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Services;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.UnitTest.ERA;

/// <summary>验证依赖边界与仓储事务保护，不连接数据库；SQL 行为由既有集成用例覆盖。</summary>
public class EraPersistenceBoundaryFixture
{
    private static VoidBotDbContext Context()
    {
        var configuration = Substitute.For<IConfiguration>();
        configuration["ConnectionStrings:Mysql"].Returns("Server=localhost;Database=era_repository_model_only;User=model_only;Password=unused");
        return new VoidBotDbContext(new ConnectionStringSetting(configuration), Substitute.For<ICurrentUser>());
    }

    [Fact]
    public void Era_services_and_data_providers_should_not_depend_on_database_infrastructure()
    {
        var types = typeof(EraEquipmentService).Assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract &&
            type.Namespace?.StartsWith("Void.Bot.Core.Services.ERA", StringComparison.Ordinal) == true &&
            (type.Name.EndsWith("Service", StringComparison.Ordinal) || type.Name.EndsWith("DataProvider", StringComparison.Ordinal))).ToList();
        types.ShouldNotBeEmpty();
        foreach (var type in types)
        {
            var dependencies = type.GetConstructors().SelectMany(constructor => constructor.GetParameters()).Select(parameter => parameter.ParameterType)
                .Concat(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Select(field => field.FieldType)).ToList();
            dependencies.Any(dependency => typeof(DbContext).IsAssignableFrom(dependency) || typeof(DbConnection).IsAssignableFrom(dependency) ||
                typeof(IUnitOfWork).IsAssignableFrom(dependency) || dependency == typeof(IServiceProvider) ||
                dependency == typeof(Repository) ||
                dependency.IsGenericType && (dependency.GetGenericTypeDefinition() == typeof(DbSet<>) ||
                    dependency.GetGenericTypeDefinition() == typeof(Repository<>))).ShouldBeFalse(type.FullName);
            if (type.Name.EndsWith("Service", StringComparison.Ordinal))
                dependencies.Any(dependency => dependency == typeof(IRepository) ||
                    dependency.IsGenericType && dependency.GetGenericTypeDefinition() == typeof(IRepository<>))
                    .ShouldBeFalse(type.FullName);
            if (type.Name.EndsWith("DataProvider", StringComparison.Ordinal))
                dependencies.Any(dependency => dependency.Name.EndsWith("DataProvider", StringComparison.Ordinal))
                    .ShouldBeFalse(type.FullName);
        }
    }

    [Fact]
    public void Independent_coordination_repository_should_be_singleton_safe()
    {
        typeof(ISingleton).IsAssignableFrom(typeof(IRepository)).ShouldBeTrue();
        typeof(Repository).GetConstructors().SelectMany(constructor => constructor.GetParameters())
            .Any(parameter => typeof(DbContext).IsAssignableFrom(parameter.ParameterType) ||
                parameter.ParameterType.IsGenericType && parameter.ParameterType.GetGenericTypeDefinition() == typeof(IRepository<>)).ShouldBeFalse();
    }

    [Fact]
    public async Task Repository_immediate_writes_should_require_the_outer_transaction()
    {
        using var context = Context();
        var repository = new Repository<EraPlayer>(context);
        Should.Throw<InvalidOperationException>(() => repository.RequireTransaction());
        await Should.ThrowAsync<InvalidOperationException>(() => repository.ExecuteSqlAsync(
            $"UPDATE era_player SET warehouse_capacity = {300} WHERE id = {Guid.NewGuid().ToString()}", CancellationToken.None));
        await Should.ThrowAsync<InvalidOperationException>(() => repository.ExecuteDeleteAsync(_ => true, CancellationToken.None));
        await Should.ThrowAsync<InvalidOperationException>(() => repository.ExecuteUpdateAsync(_ => true,
            setters => setters.SetProperty(player => player.WarehouseCapacity, 300), CancellationToken.None));
    }

    [Fact]
    public void Repository_should_keep_interpolated_lock_values_as_parameters_and_leave_entities_uncommitted()
    {
        using var context = Context();
        var repository = new Repository<EraPlayer>(context);
        var id = Guid.NewGuid().ToString();
        var sql = repository.QuerySql($"SELECT * FROM era_player WHERE id = {id} FOR UPDATE").ToQueryString();
        sql.ShouldContain("@p0");
        sql.ShouldContain("FOR UPDATE");
        repository.QueryNoTracking().Expression.ToString().ShouldContain("AsNoTracking");
        var player = new EraPlayer { Id = Guid.NewGuid(), WarehouseCapacity = 300, Version = 1 };
        repository.Insert(player);
        context.Entry(player).State.ShouldBe(EntityState.Added);
        context.Database.CurrentTransaction.ShouldBeNull();
    }
}
