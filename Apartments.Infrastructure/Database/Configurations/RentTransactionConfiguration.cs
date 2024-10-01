using Apartments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apartments.Infrastructure.Database.Configurations;

public class RentTransactionConfiguration : IEntityTypeConfiguration<RentTransaction>
{
    public void Configure(EntityTypeBuilder<RentTransaction> builder)
    {
        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Owner)
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Apartment)
            .WithMany()
            .HasForeignKey(x => x.ApartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.RentAmount)
            .HasPrecision(8, 2);

        builder.Property(a => a.OwnerId)
            .HasMaxLength(50);

        builder.Property(a => a.TenantId)
            .HasMaxLength(50);

        builder.Property(a => a.Status)
            .HasMaxLength(20);
    }
}