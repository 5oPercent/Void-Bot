using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Domain;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.Core.Data;

public class VoidBotDbContext(ConnectionStringSetting connectionString) : DbContext, IUnitOfWork
{
    private readonly string _dbConnectionString = connectionString.Mysql;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(_dbConnectionString, new MySqlServerVersion(new Version(8, 0, 3)));
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var allTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && typeof(IEntity).IsAssignableFrom(t))
            .ToList();

        foreach (var type in allTypes.Where(type => modelBuilder.Model.FindEntityType(type) == null))
        {
            modelBuilder.Model.AddEntityType(type);
        }
    }
    
    public bool ShouldSaveChanges { get; set; }
    
    public override int SaveChanges()
    {
        return ShouldSaveChanges ? base.SaveChanges() : 0;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        return ShouldSaveChanges ? base.SaveChangesAsync(cancellationToken) : Task.FromResult(0);
    }
}