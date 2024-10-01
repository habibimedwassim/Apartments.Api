using Apartments.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Apartments.Infrastructure.Database.Configurations;

public class ApartmentRequestConfiguration : IEntityTypeConfiguration<ApartmentRequest>
{
    public void Configure(EntityTypeBuilder<ApartmentRequest> builder)
    {
        builder.Property(r => r.ApartmentId)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

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

        builder.Property(a => a.OwnerId)
            .HasMaxLength(50);

        builder.Property(a => a.TenantId)
            .HasMaxLength(50);

        builder.Property(a => a.Reason)
            .HasMaxLength(500);

        builder.Property(a => a.Status)
            .HasMaxLength(20);

        builder.Property(a => a.RequestType)
            .HasMaxLength(10);
    }
}