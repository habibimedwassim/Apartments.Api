using Apartments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apartments.Infrastructure.Database.Configurations;

public class ChangeLogConfiguration : IEntityTypeConfiguration<ChangeLog>
{
    public void Configure(EntityTypeBuilder<ChangeLog> builder)
    {
        builder.Property(a => a.EntityType)
            .HasMaxLength(50);

        builder.Property(a => a.PropertyName)
            .HasMaxLength(50);

        builder.Property(a => a.PropertyId)
            .HasMaxLength(50);

        builder.Property(a => a.OldValue)
            .HasMaxLength(500);

        builder.Property(a => a.NewValue)
            .HasMaxLength(500);

        builder.Property(a => a.ChangedBy)
            .HasMaxLength(50);

        builder.HasIndex(x => x.EntityType);
        builder.HasIndex(x => x.PropertyName);
    }
}
