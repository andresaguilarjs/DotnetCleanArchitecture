using Domain.Entities.User;
using Domain.Entities.User.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(email => email.Value, email => Email.Create(email));
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(firstName => firstName.Value, firstName => new FirstName(firstName));

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(lastName => lastName.Value, lastName => new LastName(lastName));

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.LastUpdatedAt).IsRequired();
    }
}