using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Read;

internal sealed class UserReadConfiguration : IEntityTypeConfiguration<UserReadEntity>
{
    public void Configure(EntityTypeBuilder<UserReadEntity> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
    }
}
