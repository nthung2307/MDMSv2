using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Nobisoft.Core.Database.DbContext;

public class AppReadWriteDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    // TODO: How to count no. of instances
    public AppReadWriteDbContext(DbContextOptions<AppReadWriteDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // In case of creating AppReadWriteDbContext within the entry assembly 
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        // In case of AppReadWriteDbContext is in Nobi.Core.Database.EntityFramework and configuration files are placed in the entry assembly
        if (Assembly.GetEntryAssembly() != null)
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetEntryAssembly() ?? throw new NullReferenceException());
        // Get the assemblies currently loaded in the application domain
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var loadedAssembly in loadedAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(loadedAssembly);
        }
    }
}