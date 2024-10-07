using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.DBContext;

public sealed class ApplicationReadDbContext : DbContext
{
    public ApplicationReadDbContext(DbContextOptions<ApplicationReadDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationReadDbContext).Assembly, ReadConfigurationsFilter);
        base.OnModelCreating(modelBuilder);
    }

    private static bool ReadConfigurationsFilter(Type type)
    {
        return type.Namespace != null && type.Namespace.Contains("Configurations.Read");
    }
}
