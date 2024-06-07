using Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
    {
        public void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.Email).IsRequired().HasMaxLength(100);
            builder.HasIndex(x => x.Email).IsUnique();

            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);

            builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);

            builder.Property(x => x.CreatedAt).IsRequired();

            builder.Property(x => x.LastUpdatedAt).IsRequired();
        }
    }
}