using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Settings.System;
using Void.Bot.Message.Dto;

namespace Void.Bot.Core.Data;

public class VoidBotDbContext(ConnectionStringSetting connectionString, ICurrentUser currentUser) : DbContext, IUnitOfWork
{
    private readonly string _dbConnectionString = connectionString.Mysql;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseMySql(_dbConnectionString, new MySqlServerVersion(new Version(8, 0, 3)))
            .EnableDetailedErrors()
            .UseSnakeCaseNamingConvention();
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        RegisterAllEntities(modelBuilder);
    }
    
    private void RegisterAllEntities(ModelBuilder modelBuilder)
    {
        var entityTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => !t.IsAbstract &&
                        t.IsClass &&
                        typeof(IEntityBase).IsAssignableFrom(t) &&
                        modelBuilder.Model.FindEntityType(t) == null)
            .ToList();

        foreach (var type in entityTypes)
        {
            modelBuilder.Entity(type);
        }
    }
    
    public override int SaveChanges()
    {
        UpdateEntityAudit();
        UpdateCreatedFields();
        UpdateLastModifiedFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        UpdateEntityAudit();
        UpdateCreatedFields();
        UpdateLastModifiedFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateEntityAudit()
    {
        var userId = currentUser.Id ?? Guid.Empty;

        foreach (var entry in ChangeTracker.Entries()
                     .Where(e => e.Entity.GetType().GetInterfaces()
                         .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityAudit<>))))
        {
            dynamic entity = entry.Entity;
            
            switch (entry.State)
            {
                case EntityState.Added:
                    entity.CreatedBy = userId;
                    break;
                case EntityState.Modified:
                    entity.LastModifiedBy = userId;
                    break;
            }
        }
    }
    
    private void UpdateCreatedFields()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e is { State: EntityState.Added, Entity: IEntityCreated }))
        {
            if (((IEntityCreated)entry.Entity).CreatedAt == default)
            {
                ((IEntityCreated)entry.Entity).CreatedAt = DateTimeOffset.UtcNow;
            }
        }
    }
    
    private void UpdateLastModifiedFields()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e is { State: EntityState.Modified, Entity: IEntityModified }))
        {
            ((IEntityModified)entry.Entity).UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
