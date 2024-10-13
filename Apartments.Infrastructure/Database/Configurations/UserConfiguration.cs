using Apartments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Apartments.Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.CIN)
            .IsUnique();

        builder.Property(a => a.CIN)
            .HasMaxLength(8)
            .IsFixedLength();

        builder.HasIndex(u => u.SysId)
            .IsUnique();

        builder.Property(u => u.SysId)
            .ValueGeneratedOnAdd()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        builder.Property(a => a.FirstName)
            .HasMaxLength(50);

        builder.Property(a => a.LastName)
            .HasMaxLength(50);

        builder.Property(a => a.EmailCode)
            .HasMaxLength(6)
            .IsFixedLength();

        builder.Property(a => a.ResetCode)
            .HasMaxLength(6)
            .IsFixedLength();

        builder.Property(a => a.PhoneNumber)
            .HasMaxLength(8)
            .IsFixedLength();

        builder.Property(a => a.Gender)
            .HasMaxLength(10);

        builder.Property(a => a.Role)
            .HasMaxLength(5);
    }
}