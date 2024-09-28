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
    }
}