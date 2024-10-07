using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.DBContext;

public sealed class ApplicationWriteDbContext : DbContext
{
    public ApplicationWriteDbContext(DbContextOptions<ApplicationWriteDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationWriteDbContext).Assembly, WriteConfigurationsFilter);
        base.OnModelCreating(modelBuilder);
    }

    private static bool WriteConfigurationsFilter(Type type)
    {
        return type.Namespace != null && type.Namespace.Contains("Configurations.Write");
    }
}
